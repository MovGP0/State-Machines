using System;
using ActiveStateMachine.Messages;
using NLog.Fluent;

namespace ActiveStateMachine.Logging
{
    public sealed class MessageLogger : IObserver<IMessage>
    {
        public MessageLogger(string name = "")
        {
            Name = name;
        }

        private string Name { get; }

        public void OnNext(IMessage value)
        {
            var message = string.IsNullOrWhiteSpace(Name)
                ? $"Received message: {value}"
                : $"{Name}: Received message: {value}";

            Log.Trace()
                .Message(message);
        }

        public void OnError(Exception error)
        {
            var message = string.IsNullOrWhiteSpace(Name)
                ? "Encountered error."
                : $"{Name}: Encountered error.";

            Log.Error()
                .Message(message)
                .Exception(error);
        }

        public void OnCompleted()
        {
            var message = string.IsNullOrWhiteSpace(Name)
                ? "Source completed."
                : $"{Name}: Source completed.";

            Log.Trace()
                .Message(message);
        }
    }
}
