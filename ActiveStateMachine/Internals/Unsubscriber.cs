using ActiveStateMachine.Messages;

namespace ActiveStateMachine.Internals;

internal sealed class Unsubscriber : IDisposable
{
    private readonly List<IObserver<StateMachineMessage>> _list;
    private readonly IObserver<StateMachineMessage> _observer;

    public Unsubscriber(List<IObserver<StateMachineMessage>> list, IObserver<StateMachineMessage> observer)
    {
        _list = list;
        _observer = observer;
    }

    public void Dispose() => _list.Remove(_observer);
}