using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ActiveStateMachine.Messages
{
    public sealed class MessageReceiver : IObservable<StateMachineMessage>
    {
        private List<IObserver<StateMachineMessage>> Observers { get; } = new List<IObserver<StateMachineMessage>>();

        public async Task ReceiveAsync(ISourceBlock<StateMachineMessage> source)
        {
            while (await source.OutputAvailableAsync())
            {
                var message = source.Receive();
                Observers.ForEach(observer => observer.OnNext(message));
            }

            Observers.ForEach(observer => observer.OnCompleted());
        }

        public async Task ReceiveAsync(ISourceBlock<StateMachineMessage> source, CancellationToken token)
        {
            while (await source.OutputAvailableAsync(token))
            {
                var message = source.Receive(token);
                Observers.ForEach(observer => observer.OnNext(message));
            }

            Observers.ForEach(observer => observer.OnCompleted());
        }

        public IDisposable Subscribe(IObserver<StateMachineMessage> observer)
        {
            Observers.Add(observer);
            return Disposable.Create(() => Observers.Remove(observer));
        }
    }
}
