using ActiveStateMachine.States;

namespace ActiveStateMachine.Messages.Replies;

public sealed record PossibleStatesReply(string Source, string Target, IReadOnlyList<State> PossibleStates)
    : StateMachineReply(new Version(1,0), "possible-states", Source, Target, "");