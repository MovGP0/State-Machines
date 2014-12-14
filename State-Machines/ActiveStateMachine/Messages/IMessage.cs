using System;

namespace ActiveStateMachine.Messages
{
    public interface IMessage
    {
        Guid Id { get; }
        DateTime Timestamp { get; }
        Version Version { get; }
    }
}
