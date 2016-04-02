using System;

namespace ActiveStateMachine.Messages
{
    public sealed class ResumedEvent : StateMachineEvent
    {
        public ResumedEvent(string source) : base(new Version(1,0), "resumed", source, string.Empty, string.Empty)
        {
        }
    }
}