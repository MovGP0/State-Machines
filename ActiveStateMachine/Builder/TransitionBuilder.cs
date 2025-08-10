using ActiveStateMachine.Transitions;

namespace ActiveStateMachine.Builder;

public sealed class TransitionBuilder
{
    private readonly StateBuilder _parent;
    private readonly string _name;
    private readonly string _trigger;
    private string? _target;
    private readonly List<TransitionPrecondition> _preconditions = [];
    private readonly List<TransitionAction> _actions = [];

    internal TransitionBuilder(StateBuilder parent, string trigger)
    {
        _parent = parent;
        _trigger = trigger;
    }

    public TransitionBuilder When(string name, Func<bool> predicate)
    {
        _preconditions.Add(new TransitionPrecondition(name, predicate));
        return this;
    }

    public TransitionBuilder Do(string name, Action action)
    {
        _actions.Add(new TransitionAction(name, action));
        return this;
    }

    public StateBuilder GoTo(string targetStateName)
    {
        _target = targetStateName ?? throw new ArgumentNullException(nameof(targetStateName));
        _parent.Transitions.Add(new TransitionSpec(_trigger, _target, _preconditions, _actions));
        return _parent;
    }
}