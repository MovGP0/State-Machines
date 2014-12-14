using System;
using System.Collections.Generic;
using System.Linq;
using ActiveStateMachine.Messages;
using ActiveStateMachine.States;
using Anotar.NLog;

namespace ActiveStateMachine
{
    public sealed class StateMachine : IObserver<IMessage>
    {
        public IEnumerable<State> PossibleStates { get; }
        public State CurrentState => StateHistory.Last();
        public IEnumerable<State> StateHistory { get; } = new List<State>();
        public IEnumerable<IMessage> MessageHistory { get; } = new List<IMessage>();
        public StateMachineState State { get; private set; }

        public StateMachine(IEnumerable<State> possibleStates)
        {
            PossibleStates = possibleStates;
            State = StateMachineState.Initialized;
        }

        [LogToErrorOnException]
        public void Start()
        {
            if (State == StateMachineState.Initialized)
                State = StateMachineState.Running;
        }

        [LogToErrorOnException]
        public void OnNext(IMessage message)
        {
            if(State != StateMachineState.Running) return;

            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            State = StateMachineState.Stopped;
            LogTo.InfoException("Stopped because of an exception", error);
        }

        [LogToErrorOnException]
        public void OnCompleted()
        {
            State = StateMachineState.Stopped;
        }
    }
}
