using ActiveStateMachine.States;
using ActiveStateMachine.Transitions;

namespace ActiveStateMachine.Builder;

public sealed class StateMachineBuilder
{
    private readonly string machineName;
    private readonly Dictionary<string, StateBuilder> stateBuilders = new(StringComparer.Ordinal);
    private string? defaultStateName;

    public StateMachineBuilder(string machineName)
    {
        this.machineName = string.IsNullOrWhiteSpace(machineName) ? throw new ArgumentException("Machine name required.", nameof(machineName)) : machineName;
    }

    public StateBuilder State(string stateName, bool isDefault = false)
    {
        if (!stateBuilders.TryGetValue(stateName, out var sb))
        {
            sb = new StateBuilder(stateName);
            stateBuilders.Add(stateName, sb);
        }
        if (isDefault)
        {
            if (defaultStateName is not null && !string.Equals(defaultStateName, stateName, StringComparison.Ordinal))
                throw new InvalidOperationException($"Default state already set to '{defaultStateName}'.");
            defaultStateName = stateName;
            sb.IsDefault = true;
        }
        return sb;
    }

    public IReadOnlyList<State> Build()
    {
        if (defaultStateName is null)
            throw new InvalidOperationException("Exactly one default state is required.");

        var states = new List<State>(stateBuilders.Count);
        foreach (var kv in stateBuilders)
        {
            var sb = kv.Value;
            var transitions = sb.Transitions.Select(t => new Transition(
                string.Empty,
                t.Trigger,
                sb.StateName,
                t.TargetStateName,
                t.Preconditions,
                t.Actions));

            states.Add(new SimpleState(
                sb.StateName,
                transitions,
                sb.EntryActions,
                sb.ExitActions,
                sb.IsDefault));
        }

        // validate that all targets exist
        var stateNames = new HashSet<string>(stateBuilders.Keys, StringComparer.Ordinal);
        foreach (var s in states)
        foreach (var t in s.TransitionList)
            if (!stateNames.Contains(t.TargetStateName))
                throw new InvalidOperationException($"State '{s.StateName}' has transition to undefined target '{t.TargetStateName}'.");

        // ensure unique names
        if (stateBuilders.Keys.Count != stateNames.Count)
            throw new InvalidOperationException("State names must be unique.");

        return states;
    }
}