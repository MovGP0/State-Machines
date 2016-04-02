using System;

namespace ActiveStateMachine.Messages
{
    public sealed class PauseCommand : StateMachineCommand
    {
        public PauseCommand (string source, string target, string messageInfo) 
            : base (new Version (1, 0), "pause", source, target, messageInfo)
        {
        }
    }
}