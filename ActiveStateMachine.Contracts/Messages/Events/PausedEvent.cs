using System;

namespace ActiveStateMachine.Messages
{
    public sealed class PausedEvent : StateMachineEvent
    {
        public PausedEvent(string source) : base(new Version(1,0), "paused", source, string.Empty, string.Empty)
        {
        }
    }
}