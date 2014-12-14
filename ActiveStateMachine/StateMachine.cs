using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using ActiveStateMachine.Messages;
using ActiveStateMachine.States;
using Anotar.NLog;

namespace ActiveStateMachine
{
    public sealed class StateMachine : IObserver<IMessage>, IObservable<IMessage>
    {
        public string Name { get; }
        public IEnumerable<State> PossibleStates { get; }
        public State CurrentState => StateHistory.Last();
        public IEnumerable<State> StateHistory { get; } = new List<State>();
        public IEnumerable<IMessage> MessageHistory { get; } = new List<IMessage>();
        public StateMachineState State { get; private set; }
        private List<IObserver<IMessage>> Observers { get; } = new List<IObserver<IMessage>>();
        public bool CanStart => State == StateMachineState.Initialized || State == StateMachineState.Paused;
        public bool CanPause => State == StateMachineState.Running;
        public bool CanResume => State == StateMachineState.Paused;

        public StateMachine(string name, IEnumerable<State> possibleStates)
        {
            Name = name;
            PossibleStates = possibleStates;
            State = StateMachineState.Initialized;
        }

        public void Start()
        {
            if (!CanStart)
            {
                LogTo.Warn("Could not start, because current state is {0}.", State);
                return;
            }
            
            State = StateMachineState.Running;
        }

        public void Pause()
        {
            if (!CanPause)
            {
                LogTo.Warn("Could not pause, because current state is {0}.", State);
                return;
            }

            State = StateMachineState.Paused;
            MessageObservers(new StateMachineSystemMessage(Name, "paused"));
        }

        public void Resume()
        {
            if (!CanResume)
            {
                LogTo.Warn("Could not resume, because current state is {0}.", State);
                return;
            }

            State = StateMachineState.Running;
            MessageObservers(new StateMachineSystemMessage(Name, "resumed"));
        }

        [LogToErrorOnException]
        public void OnNext(IMessage message)
        {
            if(State != StateMachineState.Running) return;

            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            Stop();
            LogTo.InfoException("Stopped because of an exception", error);
        }

        public void OnCompleted()
        {
            Stop();
            MessageObservers(new StateMachineSystemMessage(Name, "completed"));
        }

        private void Stop()
        {
            State = StateMachineState.Stopped;
            MessageObservers(new StateMachineSystemMessage(Name, "stopped"));
        }

        public IDisposable Subscribe(IObserver<IMessage> observer)
        {
            Observers.Add(observer);
            return Disposable.Create(() => Observers.Remove(observer));
        }

        private void MessageObservers(IMessage message)
        {
            Observers.ForEach(observer => observer.OnNext(message));
        }
    }
}
