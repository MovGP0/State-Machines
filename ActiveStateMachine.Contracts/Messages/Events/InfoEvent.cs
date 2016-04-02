using System;

namespace ActiveStateMachine.Messages
{
    public sealed class InfoEvent : StateMachineEvent
    {
        public InfoEvent(string source, string messageInfo) : base(new Version(1,0), "info", source, string.Empty, messageInfo)
        {
        }
    }
}