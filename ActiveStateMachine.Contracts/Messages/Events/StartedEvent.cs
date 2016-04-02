using System;

namespace ActiveStateMachine.Messages
{
    public sealed class StartedEvent : StateMachineEvent
    {
        public StartedEvent(string source) : base(new Version(1,0), "started", source, string.Empty, string.Empty)
        {
        }
    }
}