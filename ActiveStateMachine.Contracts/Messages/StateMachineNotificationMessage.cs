using System;

namespace ActiveStateMachine.Messages
{
    public sealed class StateMachineNotificationMessage : StateMachineMessage
    {
        public StateMachineNotificationMessage(StateMachineInfo payload) 
            : base(new Version(1,0))
        {
            Payload = payload;
        }

        public StateMachineInfo Payload { get; }
    }
}