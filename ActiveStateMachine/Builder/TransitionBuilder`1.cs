using ActiveStateMachine.Transitions;

namespace ActiveStateMachine.Builder;

public sealed class TransitionBuilder<TTrigger>
{
    private readonly StateBuilder<TTrigger> _parent;
    private readonly string _name;
    private readonly TTrigger _trigger;
    private string? _target;
    private readonly List<TransitionPrecondition> _preconditions = [];
    private readonly List<TransitionAction> _actions = [];

    internal TransitionBuilder(StateBuilder<TTrigger> parent, TTrigger trigger)
    {
        _parent = parent;
        _trigger = trigger;
    }

    public TransitionBuilder<TTrigger> When(string name, Func<bool> predicate)
    {
        _preconditions.Add(new TransitionPrecondition(name, predicate));
        return this;
    }

    public TransitionBuilder<TTrigger> Do(string name, Action action)
    {
        _actions.Add(new TransitionAction(name, action));
        return this;
    }

    public StateBuilder<TTrigger> GoTo(string targetStateName)
    {
        _target = targetStateName ?? throw new ArgumentNullException(nameof(targetStateName));
        _parent.Transitions.Add(new TransitionSpec<TTrigger>(_trigger, _target, _preconditions, _actions));
        return _parent;
    }
}