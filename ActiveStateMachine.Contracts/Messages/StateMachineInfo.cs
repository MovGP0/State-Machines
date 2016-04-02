using System;

namespace ActiveStateMachine.Messages
{
    public abstract class StateMachineInfoMessage : StateMachineMessage
    {
        protected StateMachineInfoMessage(Version version, string name, string eventInfo, string source, string target) : base(version)
        {
            Name = name;
            EventInfo = eventInfo;
            Source = source;
            Target = target;
        }

        public string Name { get; }
        public string EventInfo { get; }
        public string Source { get; }
        public string Target { get; }
    }
}