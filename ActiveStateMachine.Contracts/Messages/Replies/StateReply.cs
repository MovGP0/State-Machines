using System;

namespace ActiveStateMachine.Messages
{
    public sealed class StateReply : StateMachineReply
    {
        public StateReply(string source, string target, StateMachineState state) 
            : base(new Version(1,0), "state", source, target, string.Empty)
        {
            State = state;
        }

        public StateMachineState State { get; }
    }
}