using System;

namespace ActiveStateMachine.Messages
{
    public sealed class StateMachineNotificationMessage : StateMachineInfoMessage
    {
        public StateMachineNotificationMessage(string name, string eventInfo, string source, string target) 
            : base(new Version(1,0), name, eventInfo, source, target)
        {
        }
    }
}