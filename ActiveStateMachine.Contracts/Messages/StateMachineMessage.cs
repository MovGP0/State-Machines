using System;

namespace ActiveStateMachine.Messages
{
    public abstract class StateMachineMessage
    {
        protected StateMachineMessage (Version version, string name, string source, string target, string messageInfo)
        {
            Version = version;
            Name = name;
            MessageInfo = messageInfo;
            Source = source;
            Target = target;
            Timestamp = DateTime.UtcNow;
            Id = new Guid ();
        }

        public Guid Id { get; }
        public DateTime Timestamp { get; }
        public Version Version { get; }
        public string Name { get; }
        public string MessageInfo { get; }
        public string Source { get; }
        public string Target { get; }
    }
}
