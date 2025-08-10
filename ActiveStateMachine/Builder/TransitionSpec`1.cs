using ActiveStateMachine.Transitions;

namespace ActiveStateMachine.Builder;

internal sealed record TransitionSpec<TTrigger>(
    TTrigger Trigger,
    string TargetStateName,
    IEnumerable<TransitionPrecondition> Preconditions,
    IEnumerable<TransitionAction> Actions);