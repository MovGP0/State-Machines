using System;

namespace ActiveStateMachine.Messages
{
    public sealed class StateMachineExternalMessage 
        : StateMachineMessage
    {
        public StateMachineExternalMessage(StateMachineInfo payload) : base(new Version(1,0))
        {
            Payload = payload;
        }

        // TODO: inline this 
        public StateMachineInfo Payload { get; }
    }
}