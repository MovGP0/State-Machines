using System;

namespace ActiveStateMachine.Messages
{
    public sealed class StateMachineSystemMessage : StateMachineMessage
    {
        public StateMachineSystemMessage(string stateMachineName, string message) 
            : base(new Version(1,0))
        {
            StateMachineName = stateMachineName;
            Message = message;
        }

        public string Message { get; }
        public string StateMachineName { get; }
    }
}