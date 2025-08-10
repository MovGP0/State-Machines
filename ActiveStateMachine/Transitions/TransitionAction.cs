namespace ActiveStateMachine.Transitions;

public sealed class TransitionAction
{
    public string Name { get; }
    private readonly Action action;

    public TransitionAction(string name, Action action)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        this.action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public void Execute() => action();
}