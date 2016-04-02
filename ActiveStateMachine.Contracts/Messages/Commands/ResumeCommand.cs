using System;

namespace ActiveStateMachine.Messages
{
    public sealed class ResumeCommand : StateMachineCommand
    {
        public ResumeCommand (string source, string target, string messageInfo) 
            : base (new Version (1, 0), "resume", source, target, messageInfo)
        {
        }
    }
}