using System;

namespace ActiveStateMachine.Messages
{
    [ToString]
    public sealed class StateMachineCommandMessage : StateMachineMessage
    {
        public StateMachineCommandMessage(StateMachineInfo payload) 
            : base(new Version(1,0))
        {
            Payload = payload;
        }

        public StateMachineInfo Payload { get; }
    }
}