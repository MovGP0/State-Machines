using System;

namespace ActiveStateMachine.Messages
{
    public sealed class CompletedEvent : StateMachineEvent
    {
        public CompletedEvent(string source) : base(new Version(1,0), "completed", source, string.Empty, string.Empty)
        {
        }
    }
}