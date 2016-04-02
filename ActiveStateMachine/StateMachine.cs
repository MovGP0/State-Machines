using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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
    public static class StateMachine
    {
        public static ISubject<StateMachineMessage, StateMachineMessage> Create(string name, IEnumerable<State> possibleStates, int queueCapacity)
        {
            // Checks
            var possibleStateCollection = possibleStates.ToArray();
            if(possibleStateCollection.Count(s => s.IsDefaultState) != 1)
            {
                throw new ArgumentException("States must have exactly one default state.", nameof(possibleStates));
            }

            var possibleStateNames = possibleStateCollection.Select(s => s.StateName).ToArray();
            if(possibleStateNames.Distinct().Count() == possibleStateNames.Count())
            {
                throw new ArgumentException("States must have distinct names.", nameof(possibleStates));
            }

            // State 
            ICollection<State> stateHistory = new List<State>();
            var state = StateMachineState.Initialized;
            var triggerQueue = new BlockingCollection<string>(queueCapacity);
            ICollection<StateMachineMessage> messageHistory = new List<StateMachineMessage>();
            var workerTask = Task.CompletedTask;
            var tokenSource = new CancellationTokenSource();
            ICollection<IObserver<StateMachineMessage>> observers = new List<IObserver<StateMachineMessage>>();
            ManualResetEvent resumer;

            // Creation
            Initialize(name, stateHistory, possibleStateCollection, observers, out resumer);

            var observer = Observer.Create<StateMachineMessage>(
                message => state = OnNext(message, name, stateHistory, possibleStateCollection, state, triggerQueue, messageHistory, ref workerTask, ref tokenSource, observers, resumer), 
                ex => state = OnError(ex, name, triggerQueue, ref workerTask, tokenSource, observers), 
                () => state = OnCompleted(name, triggerQueue, ref workerTask, tokenSource, observers));

            var observable = Observable.Create<StateMachineMessage>(
                obs => Subscribe(obs, observers));

            return Subject.Create(observer, observable);
        }
        
        private static bool CanStart(StateMachineState state)
        {
            return state == StateMachineState.Initialized || state == StateMachineState.Paused;
        }

        private static bool CanPause(StateMachineState state)
        {
            return state == StateMachineState.Running;
        }

        private static bool CanResume(StateMachineState state)
        {
            return state == StateMachineState.Paused;
        }
        
        private static State GetCurrentState(IEnumerable<State> stateHistory)
        {
            return stateHistory.Last();
        }

        private static void SetCurrentState(State state, ICollection<State> stateHistory)
        {
            stateHistory.Add(state);
        }
        
        private static void WorkerMethod(string name, ICollection<State> stateHistory, ICollection<State> possibleStates, BlockingCollection<string> triggerQueue, 
            CancellationTokenSource tokesSource, ICollection<IObserver<StateMachineMessage>> observers, WaitHandle resumer)
        {
            resumer.WaitOne();

            try
            {
                triggerQueue.GetConsumingEnumerable(tokesSource.Token)
                    .SelectMany(trigger => GetCurrentState(stateHistory).StateTransitionList.Where(t => t.Name == trigger))
                    .ForEach(transition => ExecuteTransition(transition, name, stateHistory, possibleStates, observers));

                if(tokesSource.IsCancellationRequested)
                {
                    MessageObservers(new InfoEvent(name, "cancelled"), observers);
                }
            }
            catch(TaskCanceledException)
            {
                MessageObservers(new InfoEvent(name, "cancelled"), observers);
            }
        }

        private static void ExecuteTransition(Transition transition, string name, ICollection<State> stateHistory, ICollection<State> possibleStates, 
            ICollection<IObserver<StateMachineMessage>> observers)
        {
            if(!EnsureAllPreconditionsMetAndMessageObserversWhenNotMet(transition, name, stateHistory, possibleStates, observers))
                return;

            MessageObservers(new InfoEvent(name, "leaving state"), observers);
            GetCurrentState(stateHistory).ExitActions.ForEach(a => a.Execute());

            MessageObservers(new InfoEvent(name, "transitioning state"), observers);
            transition.TransitionActions.ForEach(a => a.Execute());

            MessageObservers(new InfoEvent(name, "entering state"), observers);
            SetCurrentState(possibleStates.Single(s => s.StateName == transition.TargetStateName), stateHistory);
            GetCurrentState(stateHistory).EntryActions.ForEach(a => a.Execute());
        }

        private static bool EnsureAllPreconditionsMetAndMessageObserversWhenNotMet(Transition transition, string name, IEnumerable<State> stateHistory, 
            IEnumerable<State> possibleStates, ICollection<IObserver<StateMachineMessage>> observers)
        {
            if(GetCurrentState(stateHistory).StateName != transition.SourceStateName)
            {
                MessageObservers(new ErrorEvent(name, "Transition was in wrong state."), observers);
                return false;
            }

            if(!possibleStates.Select(s => s.StateName).Contains(transition.Name))
            {
                MessageObservers(new ErrorEvent(name, "Can not transition to target state."), observers);
                return false;
            }

            var notMetPrecondition = transition.Preconditions.FirstOrDefault(p => p.IsValid);
            if(notMetPrecondition != null)
            {
                var message = $"Can not transition to target state, because precondition {notMetPrecondition.Name} was not met.";
                MessageObservers(new ErrorEvent(name, message), observers);
                return false;
            }

            Log.Trace()
                .Message("Preconditions to for state change are met.")
                .Write();

            return true;
        }

        private static void EnterTrigger(string triggerName, BlockingCollection<string> triggerQueue)
        {
            triggerQueue.Add(triggerName);
        }
        
        private static StateMachineState Start(string name, ICollection<State> stateHistory, ICollection<State> possibleStates, StateMachineState state, 
            BlockingCollection<string> triggerQueue, ref Task workerTask, ref CancellationTokenSource tokenSource, ICollection<IObserver<StateMachineMessage>> observers, 
            WaitHandle resumer)
        {
            if(!CanStart(state))
            {
                LogTo.Warn("Could not start, because current state is {0}.", state);
                return state;
            }

            tokenSource = new CancellationTokenSource();
            var ts = tokenSource;
            workerTask = Task.Factory.StartNew(() => WorkerMethod(name, stateHistory, possibleStates, triggerQueue, ts, observers, resumer), tokenSource.Token, 
                TaskCreationOptions.LongRunning, TaskScheduler.Current);
            MessageObservers(new StartedEvent(name), observers);
            return StateMachineState.Running;
        }

        private static StateMachineState Pause(string name, StateMachineState state, ICollection<IObserver<StateMachineMessage>> observers, EventWaitHandle resumer)
        {
            if(!CanPause(state))
            {
                LogTo.Warn("Could not pause, because current state is {0}.", state);
                return state;
            }
            
            resumer.Reset();
            MessageObservers(new PausedEvent(name), observers);
            return StateMachineState.Paused;
        }

        private static void Initialize(string name, ICollection<State> stateHistory, IEnumerable<State> possibleStates, ICollection<IObserver<StateMachineMessage>> observers, 
            out ManualResetEvent resumer)
        {
            SetCurrentState(possibleStates.Single(s => s.IsDefaultState), stateHistory);
            resumer = new ManualResetEvent(true);
            MessageObservers(new InitializedEvent(name), observers);
        }

        private static StateMachineState Resume(string name, StateMachineState state, ICollection<IObserver<StateMachineMessage>> observers, EventWaitHandle resumer)
        {
            if(!CanResume(state))
            {
                Log.Warn()
                    .Message("Could not resume, because current state is {0}.", state)
                    .Write();

                return state;
            }

            resumer.Set();
            MessageObservers(new ResumedEvent(name), observers);
            return StateMachineState.Running;
        }

        [LogToErrorOnException]
        private static StateMachineState OnNext(StateMachineMessage message, 
            string name, ICollection<State> stateHistory, ICollection<State> possibleStates, StateMachineState state, BlockingCollection<string> triggerQueue, 
            ICollection<StateMachineMessage> messageHistory, ref Task workerTask, ref CancellationTokenSource tokenSource, ICollection<IObserver<StateMachineMessage>> observers, 
            EventWaitHandle resumer)
        {
            if(state != StateMachineState.Running)
                return state;
            if(message.Target != name)
                return state;

            messageHistory.Add(message);

            var command = message as StateMachineCommand;
            if (command != null)
            {
                return HandleStateMachineCommand(command, name, stateHistory, possibleStates, state, triggerQueue, ref workerTask, ref tokenSource, observers, resumer);
            }
            
            var request = message as StateMachineReqest;
            if (request != null)
            {
                HandleStateMachineReqest(request, name, stateHistory, possibleStates, state, observers);
            }

            return state;
        }

        private static StateMachineState HandleStateMachineCommand(StateMachineCommand message, string name, ICollection<State> stateHistory, ICollection<State> possibleStates, 
            StateMachineState state, BlockingCollection<string> triggerQueue, ref Task workerTask, ref CancellationTokenSource tokenSource, 
            ICollection<IObserver<StateMachineMessage>> observers, EventWaitHandle resumer)
        {
            if (message is StartCommand)
            {
                return Start(name, stateHistory, possibleStates, state, triggerQueue, ref workerTask, ref tokenSource, observers, resumer);
            }

            if (message is PauseCommand)
            {
                return Pause(name, state, observers, resumer);
            }

            if (message is ResumeCommand)
            {
                return Resume(name, state, observers, resumer);
            }

            if (message is StopCommand)
            {
                return Stop(name, triggerQueue, ref workerTask, tokenSource, observers);
            }
            
            EnterTrigger(message.Name, triggerQueue);
            return state;
        }

        private static void HandleStateMachineReqest(StateMachineReqest message, string name, IEnumerable<State> stateHistory, 
            IEnumerable<State> possibleStates, StateMachineState state, ICollection<IObserver<StateMachineMessage>> observers)
        {
            var stateQuery = message as GetStateRequest;
            if (stateQuery != null)
            {
                ReplyState(stateQuery, name, state, observers);
                return;
            }

            var stateQueryHistory = message as GetStateHistoryRequest;
            if (stateQueryHistory != null)
            {
                ReplyStateHistory(stateQueryHistory, name, stateHistory, observers);
                return;
            }

            var possibleStatesQuery = message as GetPossibleStatesRequest;
            if (possibleStatesQuery != null)
            {
                ReplyPossibleStates(possibleStatesQuery, name, possibleStates, observers);
                return;
            }

            Log.Warn()
                .Message("Request {0} is not supporrted.", message.Name)
                .Write();
        }

        private static void ReplyState(StateMachineMessage query, string name, StateMachineState state, ICollection<IObserver<StateMachineMessage>> observers)
        {
            MessageObservers(new StateReply(name, query.Name, state), observers);
        }

        private static void ReplyStateHistory(StateMachineMessage query, string name, IEnumerable<State> stateHistory, ICollection<IObserver<StateMachineMessage>> observers)
        {
            MessageObservers(new StateHistoryReply(name, query.Name, stateHistory), observers);
        }

        private static void ReplyPossibleStates(StateMachineMessage query, string name, IEnumerable<State> possibleStates, ICollection<IObserver<StateMachineMessage>> observers)
        {
            MessageObservers(new PossibleStatesReply(name, query.Name, possibleStates), observers);
        }

        private static StateMachineState OnError(Exception error, string name, BlockingCollection<string> triggerQueue, ref Task workerTask, CancellationTokenSource tokenSource, ICollection<IObserver<StateMachineMessage>> observers)
        {
            var newState = Stop(name, triggerQueue, ref workerTask, tokenSource, observers);

            Log.Info()
                .Message("Stopped because of an exception")
                .Exception(error)
                .Write();

            return newState;
        }

        private static StateMachineState OnCompleted(string name, BlockingCollection<string> triggerQueue, ref Task workerTask, CancellationTokenSource tokenSource, 
            ICollection<IObserver<StateMachineMessage>> observers)
        {
            var newState = Stop(name, triggerQueue, ref workerTask, tokenSource, observers);
            MessageObservers(new CompletedEvent(name), observers);
            return newState;
        }

        private static StateMachineState Stop(string name, BlockingCollection<string> triggerQueue, ref Task workerTask, CancellationTokenSource tokenSource, 
            ICollection<IObserver<StateMachineMessage>> observers)
        {
            tokenSource.Cancel();
            workerTask.Wait();
            triggerQueue.Dispose();

            MessageObservers(new StoppedEvent(name), observers);
            return StateMachineState.Stopped;
        }

        private static IDisposable Subscribe(IObserver<StateMachineMessage> observer, ICollection<IObserver<StateMachineMessage>> observers)
        {
            observers.Add(observer);
            return Disposable.Create(() => observers.Remove(observer));
        }

        private static void MessageObservers(StateMachineMessage message, ICollection<IObserver<StateMachineMessage>> observers)
        {
            observers.ForEach(observer => observer.OnNext(message));
        }
        
    }
}
