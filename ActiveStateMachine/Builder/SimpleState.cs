using ActiveStateMachine.States;
using ActiveStateMachine.Transitions;

namespace ActiveStateMachine.Builder;

public sealed class SimpleState : State
{
    public SimpleState(
        string stateName,
        IEnumerable<Transition> transitions,
        IEnumerable<TransitionAction>? entryActions = null,
        IEnumerable<TransitionAction>? exitActions = null,
        bool isDefaultState = false)
        : base(stateName, transitions, entryActions, exitActions, isDefaultState) { }
}