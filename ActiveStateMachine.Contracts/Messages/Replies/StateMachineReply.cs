using System;

namespace ActiveStateMachine.Messages
{
    public abstract class StateMachineReply : StateMachineMessage
    {
        protected StateMachineReply(Version version, string name, string source, string target, string messageInfo) : base(version, name, source, target, messageInfo)
        {
        }
    }
}