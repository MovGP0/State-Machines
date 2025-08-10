namespace ActiveStateMachine.Transitions;

public sealed class TransitionPrecondition
{
    public string Name { get; }
    private readonly Func<bool> isValid;

    public TransitionPrecondition(string name, Func<bool> isValid)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        this.isValid = isValid ?? throw new ArgumentNullException(nameof(isValid));
    }

    public bool IsValid => isValid();
}