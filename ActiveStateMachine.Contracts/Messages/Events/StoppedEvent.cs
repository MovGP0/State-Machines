using System;

namespace ActiveStateMachine.Messages
{
    public sealed class StoppedEvent : StateMachineEvent
    {
        public StoppedEvent(string source) : base(new Version(1,0), "stopped", source, string.Empty, string.Empty)
        {
        }
    }
}