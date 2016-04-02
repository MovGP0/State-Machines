using System;

namespace ActiveStateMachine.Messages
{
    public sealed class ErrorEvent : StateMachineEvent
    {
        public ErrorEvent(string source, string messageInfo) : base(new Version(1,0), "error", source, string.Empty, messageInfo)
        {
        }
    }
}