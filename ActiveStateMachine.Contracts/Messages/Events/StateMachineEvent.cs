using System;

namespace ActiveStateMachine.Messages
{
    public abstract class StateMachineEvent : StateMachineMessage
    {
        protected StateMachineEvent(Version version, string name, string source, string target, string messageInfo) 
            : base(version, name, source, target, messageInfo)
        {
        }
    }
}