using System;

namespace ActiveStateMachine.Messages
{
    public abstract class StateMachineReqest : StateMachineMessage
    {
        protected StateMachineReqest(Version version, string name, string source, string target, string messageInfo) : base(version, name, source, target, messageInfo)
        {
        }
    }
}