using System;

namespace ActiveStateMachine.Messages
{
    public sealed class WarnEvent : StateMachineEvent
    {
        public WarnEvent(string source, string messageInfo) : base(new Version(1,0), "warn", source, string.Empty, messageInfo)
        {
        }
    }
}