using System;

namespace ActiveStateMachine.Messages
{
    public abstract class StateMachineCommand : StateMachineMessage
    {
        protected StateMachineCommand(Version version, string name, string source, string target, string messageInfo) 
            : base(version, name, source, target, messageInfo)
        {
        }
    }
}