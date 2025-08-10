namespace ActiveStateMachine.Transitions;

public sealed class Transition
{
    public string Name { get; set; }
    public string Trigger { get; }
    public string SourceStateName { get; }
    public string TargetStateName { get; }
    public IReadOnlyList<TransitionPrecondition> Preconditions { get; }
    public IReadOnlyList<TransitionAction> TransitionActions { get; }

    public Transition(
        string name,
        string trigger,
        string sourceStateName,
        string targetStateName,
        IEnumerable<TransitionPrecondition> preconditions,
        IEnumerable<TransitionAction> transitionActions)
    {
        Name = name;
        Trigger = trigger!;
        SourceStateName = sourceStateName;
        TargetStateName = targetStateName;
        Preconditions = [..preconditions];
        TransitionActions = [..transitionActions];
    }
}