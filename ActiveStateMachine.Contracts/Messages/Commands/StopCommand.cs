using System;

namespace ActiveStateMachine.Messages
{
    public sealed class StopCommand : StateMachineCommand
    {
        public StopCommand (string source, string target, string messageInfo) 
            : base (new Version (1, 0), "StopCommand", source, target, messageInfo)
        {
        }
    }
}