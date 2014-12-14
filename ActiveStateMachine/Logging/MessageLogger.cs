using System;
using ActiveStateMachine.Messages;
using Anotar.NLog;

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
                ? string.Format("Received message: {0}", value)
                : string.Format("{0}: Received message: {1}", Name, value);

            LogTo.Trace(message);
        }

        public void OnError(Exception error)
        {
            var message = string.IsNullOrWhiteSpace(Name)
                ? "Encountered error."
                : string.Format("{0}: Encountered error.", Name);

            LogTo.ErrorException(message, error);
        }

        public void OnCompleted()
        {
            var message = string.IsNullOrWhiteSpace(Name)
                ? "Source completed."
                : string.Format("{0}: Source completed.", Name);

            LogTo.Trace(message);
        }
    }
}
