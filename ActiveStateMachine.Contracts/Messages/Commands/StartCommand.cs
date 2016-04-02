using System;

namespace ActiveStateMachine.Messages
{
    public sealed class StartCommand : StateMachineCommand
    {
        public StartCommand (string source, string target, string messageInfo) 
            : base (new Version (1, 0), "start", source, target, messageInfo)
        {
        }
    }
}