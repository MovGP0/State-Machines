using ActiveStateMachine.Transitions;

namespace ActiveStateMachine.Builder;

public sealed class StateBuilder
{
    internal string StateName { get; }
    internal bool IsDefault { get; set; }
    internal bool IsTerminal { get; private set; }
    internal List<TransitionAction> EntryActions { get; } = [];
    internal List<TransitionAction> ExitActions { get; } = [];
    internal List<TransitionSpec> Transitions { get; } = [];

    internal StateBuilder(string stateName) => StateName = stateName;

    public StateBuilder AsTerminal()
    {
        IsTerminal = true;
        return this;
    }

    public StateBuilder OnEnter(string name, Action action)
    {
        EntryActions.Add(new TransitionAction(name, action));
        return this;
    }

    public StateBuilder OnExit(string name, Action action)
    {
        ExitActions.Add(new TransitionAction(name, action));
        return this;
    }

    public TransitionBuilder On(string trigger) => new(this, trigger);
}