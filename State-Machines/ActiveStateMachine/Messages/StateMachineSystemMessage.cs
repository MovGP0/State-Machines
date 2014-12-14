using System;

namespace ActiveStateMachine.Messages
{
    [ToString]
    public sealed class StateMachineSystemMessage : StateMachineMessage
    {
        public StateMachineSystemMessage(StateMachineInfo payload) 
            : base(new Version(1,0))
        {
            Payload = payload;
        }

        public StateMachineInfo Payload { get; }
    }
}