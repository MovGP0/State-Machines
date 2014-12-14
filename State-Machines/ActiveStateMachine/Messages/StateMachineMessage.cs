using System;

namespace ActiveStateMachine.Messages
{
    public abstract class StateMachineMessage : IMessage
    {
        protected StateMachineMessage(Version version)
        {
            Version = version;
            Timestamp = DateTime.UtcNow;
            Id = new Guid();
        }

        public Guid Id { get; }

        public DateTime Timestamp { get; }

        public Version Version { get; }
    }
}
