using System;

namespace ActiveStateMachine.Messages
{
    public sealed class InitializeCommand : StateMachineCommand
    {
        public InitializeCommand (string source, string target, string messageInfo) 
            : base (new Version (1, 0), "initialize", source, target, messageInfo)
        {
        }
    }
}