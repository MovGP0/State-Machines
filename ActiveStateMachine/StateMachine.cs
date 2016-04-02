using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using ActiveStateMachine.Messages;
using ActiveStateMachine.States;
using ActiveStateMachine.Transitions;
using Anotar.NLog;
using MoreLinq;
using NLog.Fluent;

namespace ActiveStateMachine
{
    public sealed class StateMachine : IObserver<StateMachineMessage>, IObservable<StateMachineMessage>, IDisposable
    {
        public string Name { get; }
        public IEnumerable<State> PossibleStates { get; }

        public State CurrentState
        {
            get { return StateHistory.Last(); }
            set { StateHistory = new List<State>(StateHistory) { value }; }
        }

        public IEnumerable<State> StateHistory { get; private set; } = new List<State>();
        public IList<StateMachineMessage> MessageHistory { get; } = new List<StateMachineMessage>();
        public StateMachineState State { get; private set; }
        private List<IObserver<StateMachineMessage>> Observers { get; } = new List<IObserver<StateMachineMessage>>();

        public bool CanStart => State == StateMachineState.Initialized || State == StateMachineState.Paused;
        public bool CanPause => State == StateMachineState.Running;
        public bool CanResume => State == StateMachineState.Paused;
        
        private Task WorkerTask { get; set; }
        private CancellationTokenSource TokenSource { get; set; }

        private void WorkerMethod()
        {
            Resumer.WaitOne();

            try
            {
                TriggerQueue.GetConsumingEnumerable(TokenSource.Token)
                    .SelectMany(trigger => CurrentState.StateTransitionList.Where(t => t.Name == trigger))
                    .ForEach(ExecuteTransition);
                
                if (TokenSource.IsCancellationRequested)
                {
                    MessageObservers(new InfoEvent(Name, "cancelled"));
                }
            }
            catch(TaskCanceledException)
            {
                MessageObservers(new InfoEvent(Name, "cancelled"));
                Start();
            }
        }

        private void ExecuteTransition(Transition transition)
        {
            if (!EnsureAllPreconditionsMetAndMessageObserversWhenNotMet(transition)) return;

            MessageObservers(new InfoEvent(Name, "leaving state"));
            CurrentState.ExitActions.ForEach(a => a.Execute());

            MessageObservers(new InfoEvent(Name, "transitioning state"));
            transition.TransitionActions.ForEach(a => a.Execute());

            MessageObservers(new InfoEvent(Name, "entering state"));
            CurrentState = PossibleStates.Single(s => s.StateName == transition.TargetStateName);
            CurrentState.EntryActions.ForEach(a => a.Execute());
        }

        private bool EnsureAllPreconditionsMetAndMessageObserversWhenNotMet(Transition transition)
        {
            if (CurrentState.StateName != transition.SourceStateName)
            {
                MessageObservers(new ErrorEvent(Name, "Transition was in wrong state."));
                return false;
            }

            if (!PossibleStates.Select(s => s.StateName).Contains(transition.Name))
            {
                MessageObservers(new ErrorEvent(Name, "Can not transition to target state."));
                return false;
            }

            var notMetPrecondition = transition.Preconditions.FirstOrDefault(p => p.IsValid);
            if (notMetPrecondition != null)
            {
                MessageObservers(new ErrorEvent(Name,
                    $"Can not transition to target state, because precondition {notMetPrecondition.Name} was not met."));
                return false;
            }

            return true;
        }

        private void EnterTrigger(string triggerName)
        {
            TriggerQueue.Add(triggerName);
        }

        private ManualResetEvent Resumer { get; set; }

        private readonly BlockingCollection<string> _triggerQueue;
        // ReSharper disable once ConvertToAutoPropertyWhenPossible
        private BlockingCollection<string> TriggerQueue => _triggerQueue;

        public StateMachine(string name, IEnumerable<State> possibleStates, int queueCapacity)
        {
            var possibleStateArray = possibleStates as State[] ?? possibleStates.ToArray();
            if (possibleStateArray.Count(s => s.IsDefaultState) != 1)
            {
                throw new ArgumentException("States must have exactly one default state.", nameof(possibleStates));
            }

            var possibleStateNames = possibleStateArray.Select(s => s.StateName).ToArray();
            if (possibleStateNames.Distinct().Count() == possibleStateNames.Count())
            {
                throw new ArgumentException("States must have distinct names.", nameof(possibleStates));
            }

            Name = name;
            PossibleStates = possibleStateArray;
            _triggerQueue = new BlockingCollection<string>(queueCapacity);
            State = StateMachineState.Initialized;
        }

        private void Start()
        {
            if (!CanStart)
            {
                LogTo.Warn("Could not start, because current state is {0}.", State);
                return;
            }
            
            TokenSource = new CancellationTokenSource();
            WorkerTask = Task.Factory.StartNew(WorkerMethod, TokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);

            State = StateMachineState.Running;
            MessageObservers(new StartedEvent(Name));
        }
        
        private void Pause()
        {
            if (!CanPause)
            {
                LogTo.Warn("Could not pause, because current state is {0}.", State);
                return;
            }

            State = StateMachineState.Paused;
            Resumer.Reset();
            MessageObservers(new PausedEvent(Name));
        }

        public void Initialize()
        {
            CurrentState = PossibleStates.Single(s => s.IsDefaultState);
            Resumer = new ManualResetEvent(true);
            MessageObservers(new InitializedEvent(Name));
        }

        public void Resume()
        {
            if (!CanResume)
            {
                Log.Warn()
                    .Message("Could not resume, because current state is {0}.", State)
                    .Write();

                return;
            }

            Resumer.Set();
            State = StateMachineState.Running;
            MessageObservers(new ResumedEvent(Name));
        }

        [LogToErrorOnException]
        public void OnNext(StateMachineMessage message)
        {
            if (State != StateMachineState.Running) return;
            if (message.Target != Name) return;
            
            if (message is StartCommand) Start();
            if (message is PauseCommand) Pause();
            if (message is ResumeCommand) Resume();
            if (message is StopCommand) Stop(); 
            if (message is StateMachineCommand) EnterTrigger(message.Name);
        }
        
        public void OnError(Exception error)
        {
            Stop();

            Log.Info()
                .Message("Stopped because of an exception")
                .Exception(error)
                .Write();
        }

        public void OnCompleted()
        {
            Stop();
            MessageObservers(new CompletedEvent(Name));
        }

        private void Stop()
        {
            TokenSource.Cancel();
            WorkerTask.Wait();
            Dispose();

            State = StateMachineState.Stopped;
            MessageObservers(new StoppedEvent(Name));
        }
         
        public IDisposable Subscribe(IObserver<StateMachineMessage> observer)
        {
            Observers.Add(observer);
            return Disposable.Create(() => Observers.Remove(observer));
        }

        private void MessageObservers(StateMachineMessage message)
        {
            Observers.ForEach(observer => observer.OnNext(message));
        }

        private bool _isDisposed;
        public void Dispose()
        {
            Dispose(!_isDisposed);
            _isDisposed = true;
        }

        private void Dispose(bool disposing)
        {
            if (!disposing) return;
            _triggerQueue.Dispose();
        }
    }
}
