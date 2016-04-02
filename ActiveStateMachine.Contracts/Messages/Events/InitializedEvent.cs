using System;

namespace ActiveStateMachine.Messages
{
    public sealed class InitializedEvent : StateMachineEvent
    {
        public InitializedEvent(string source) : base(new Version(1,0), "initialized", source, string.Empty, string.Empty)
        {
        }
    }
}