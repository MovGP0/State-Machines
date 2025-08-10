using ActiveStateMachine.Transitions;

namespace ActiveStateMachine.States;

public abstract class State<TTrigger>
{
    protected State(
        string stateName,
        IEnumerable<Transition<TTrigger>> transitions,
        IEnumerable<TransitionAction>? entryActions = null,
        IEnumerable<TransitionAction>? exitActions = null,
        bool isDefaultState = false)
    {
        StateName = stateName ?? throw new ArgumentNullException(nameof(stateName));
        TransitionList = transitions?.ToList() ?? throw new ArgumentNullException(nameof(transitions));
        EntryActions = (entryActions ?? []).ToList();
        ExitActions = (exitActions ?? []).ToList();
        IsDefaultState = isDefaultState;
    }

    public string StateName { get; }
    public IReadOnlyList<Transition<TTrigger>> TransitionList { get; }
    public IReadOnlyList<TransitionAction> EntryActions { get; }
    public IReadOnlyList<TransitionAction> ExitActions { get; }
    public bool IsDefaultState { get; }
}