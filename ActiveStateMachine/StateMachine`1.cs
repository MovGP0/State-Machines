using System.Threading.Channels;
using ActiveStateMachine.Internals;
using ActiveStateMachine.Messages;
using ActiveStateMachine.States;
using Microsoft.Extensions.Logging;
using ActiveStateMachine.Messages.Commands;
using ActiveStateMachine.Messages.Events;
using ActiveStateMachine.Messages.Replies;
using ActiveStateMachine.Messages.Requests;

namespace ActiveStateMachine;

public sealed class StateMachine<TTrigger> : IObservable<StateMachineMessage>, IDisposable
{
    private readonly string _name;
    private readonly List<State<TTrigger>> _possibleStates;
    private readonly List<State<TTrigger>> _stateHistory = [];
    private readonly List<StateMachineMessage> _messageHistory = [];
    private readonly List<IObserver<StateMachineMessage>> _observers = [];
    private readonly Channel<string> _triggerChannel;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly AsyncManualResetEvent _pauseGate = new(isSet: true);
    private readonly ILogger _logger;

    private Task? _workerTask;
    private volatile StateMachineState _state = StateMachineState.Initialized;

    public StateMachine(string name, IEnumerable<State<TTrigger>> possibleStates, int queueCapacity, ILogger logger)
    {
        _name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Name must be provided", nameof(name)) : name;
        _possibleStates = (possibleStates ?? throw new ArgumentNullException(nameof(possibleStates))).ToList();

        if (_possibleStates.Count(s => s.IsDefaultState) != 1)
            throw new ArgumentException("States must include exactly one default state.", nameof(possibleStates));

        if (_possibleStates.Select(s => s.StateName).Distinct(StringComparer.Ordinal).Count() != _possibleStates.Count)
            throw new ArgumentException("States must have distinct names.", nameof(possibleStates));

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var options = new BoundedChannelOptions(queueCapacity)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        };
        _triggerChannel = Channel.CreateBounded<string>(options);

        // initialize
        SetCurrentState(_possibleStates.Single(s => s.IsDefaultState));
        Notify(new InitializedEvent(_name));
    }

    public string Name => _name;
    public StateMachineState CurrentState => _state;
    public IReadOnlyList<State<TTrigger>> StateHistory => _stateHistory;
    public IReadOnlyList<State<TTrigger>> PossibleStates => _possibleStates;
    public IReadOnlyList<StateMachineMessage> MessageHistory => _messageHistory;

    public void Start()
    {
        if (!(_state is StateMachineState.Initialized or StateMachineState.Paused))
        {
            StateMachineLog.InvalidLifecycleOperation(_logger, "start", _state);
            return;
        }

        _pauseGate.Set(); // ensure open
        _workerTask ??= Task.Run(() => WorkerLoopAsync(_cancellationTokenSource.Token));
        _state = StateMachineState.Running;
        Notify(new StartedEvent(_name));
    }

    public void Pause()
    {
        if (_state != StateMachineState.Running)
        {
            StateMachineLog.InvalidLifecycleOperation(_logger, "pause", _state);
            return;
        }
        _pauseGate.Reset();
        _state = StateMachineState.Paused;
        Notify(new PausedEvent(_name));
    }

    public void Resume()
    {
        if (_state != StateMachineState.Paused)
        {
            StateMachineLog.InvalidLifecycleOperation(_logger, "resume", _state);
            return;
        }
        _pauseGate.Set();
        _state = StateMachineState.Running;
        Notify(new ResumedEvent(_name));
    }

    public async Task StopAsync()
    {
        if (_state is StateMachineState.Stopped or StateMachineState.Completed) return;
        _cancellationTokenSource.Cancel();
        _triggerChannel.Writer.TryComplete();
        if (_workerTask is not null)
        {
            try { await _workerTask.ConfigureAwait(false); } catch { /* ignored */ }
        }
        _state = StateMachineState.Stopped;
        Notify(new StoppedEvent(_name));
    }

    public async Task FireAsync(string triggerName, CancellationToken cancellationToken = default)
    {
        if (_state != StateMachineState.Running) return;
        await _triggerChannel.Writer.WriteAsync(triggerName, cancellationToken).ConfigureAwait(false);
        StateMachineLog.TriggerEnqueued(_logger, triggerName);
    }

    public void Send(StateMachineMessage message)
    {
        if (!string.Equals(message.Target, _name, StringComparison.Ordinal)) return;

        switch (message)
        {
            case StartCommand:
                Start();
                break;
            case PauseCommand:
                Pause();
                break;
            case ResumeCommand:
                Resume();
                break;
            case StopCommand:
                _ = StopAsync();
                break;
            case FireTriggerCommand fire:
                _ = FireAsync(fire.TriggerName);
                break;
            case GetStateRequest reqState:
                Reply(new StateReply(_name, reqState.Source, _state));
                break;
            case GetStateHistoryRequest reqHist:
                Reply(new StateHistoryReply<TTrigger>(_name, reqHist.Source, _stateHistory.ToList()));
                break;
            case GetPossibleStatesRequest reqPoss:
                Reply(new PossibleStatesReply<TTrigger>(_name, reqPoss.Source, _possibleStates.ToList()));
                break;
            case GetMessageHistoryRequest reqMsg:
                var take = reqMsg.MaximumCount is null ? _messageHistory.Count : Math.Max(0, Math.Min(_messageHistory.Count, reqMsg.MaximumCount.Value));
                var slice = _messageHistory.Skip(Math.Max(0, _messageHistory.Count - take)).ToList();
                Reply(new MessageHistoryReply(_name, reqMsg.Source, slice));
                break;
            default:
                break;
        }
    }

    private async Task WorkerLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await foreach (var trigger in _triggerChannel.Reader.ReadAllAsync(cancellationToken))
            {
                await _pauseGate.WaitAsync(cancellationToken).ConfigureAwait(false);

                var current = GetCurrentState();
                var transitions = current.TransitionList.Where(t => string.Equals(t.Name, trigger, StringComparison.Ordinal)).ToList();

                foreach (var transition in transitions)
                {
                    if (!string.Equals(current.StateName, transition.SourceStateName, StringComparison.Ordinal))
                    {
                        StateMachineLog.TransitionWrongState(_logger, transition.Name, current.StateName, transition.SourceStateName);
                        Notify(new ErrorEvent(_name, "Transition was in wrong state.", new InvalidOperationException("Wrong source state")));
                        continue;
                    }

                    var notMet = transition.Preconditions.FirstOrDefault(p => !p.IsValid);
                    if (notMet is not null)
                    {
                        StateMachineLog.PreconditionFailed(_logger, transition.Name, notMet.Name);
                        Notify(new ErrorEvent(_name, $"Precondition {notMet.Name} not met.", new InvalidOperationException("Precondition failed")));
                        continue;
                    }

                    StateMachineLog.PreconditionsMet(_logger, transition.Name);

                    // Exit actions
                    Notify(new InfoEvent(_name, "leaving state"));
                    foreach (var action in current.ExitActions) action.Execute();

                    // Transition actions
                    StateMachineLog.TransitionExecuting(_logger, transition.Name, transition.SourceStateName, transition.TargetStateName);
                    Notify(new InfoEvent(_name, "transitioning state"));
                    foreach (var action in transition.TransitionActions) action.Execute();

                    // Enter next state
                    var target = _possibleStates.Single(s => string.Equals(s.StateName, transition.TargetStateName, StringComparison.Ordinal));
                    SetCurrentState(target);
                    Notify(new InfoEvent(_name, "entering state"));
                    foreach (var action in target.EntryActions) action.Execute();
                }
            }

            _state = StateMachineState.Completed;
            Notify(new CompletedEvent(_name));
        }
        catch (OperationCanceledException)
        {
            // stopping
        }
        catch (Exception ex)
        {
            StateMachineLog.WorkerException(_logger, ex);
            await StopAsync().ConfigureAwait(false);
            Notify(new ErrorEvent(_name, "Unhandled worker exception.", ex));
        }
    }

    private State<TTrigger> GetCurrentState() => _stateHistory[^1];

    private void SetCurrentState(State<TTrigger> newState)
    {
        _stateHistory.Add(newState);
    }

    private void Notify(StateMachineMessage message)
    {
        _messageHistory.Add(message);
        foreach (var observer in _observers.ToArray())
        {
            observer.OnNext(message);
        }
    }

    private void Reply(StateMachineMessage message) => Notify(message);

    public IDisposable Subscribe(IObserver<StateMachineMessage> observer)
    {
        if (observer is null) throw new ArgumentNullException(nameof(observer));
        _observers.Add(observer);
        return new Unsubscriber(_observers, observer);
    }

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _triggerChannel.Writer.TryComplete();
        _cancellationTokenSource.Dispose();
    }
}