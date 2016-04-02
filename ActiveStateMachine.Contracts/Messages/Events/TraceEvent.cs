using System;

namespace ActiveStateMachine.Messages
{
    public sealed class TraceEvent : StateMachineEvent
    {
        public TraceEvent(string source, string messageInfo) : base(new Version(1,0), "trace", source, string.Empty, messageInfo)
        {
        }
    }
}