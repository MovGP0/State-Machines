using ActiveStateMachine.Transitions;

namespace ActiveStateMachine.Builder;

internal sealed record TransitionSpec(
    string Trigger,
    string TargetStateName,
    IEnumerable<TransitionPrecondition> Preconditions,
    IEnumerable<TransitionAction> Actions);