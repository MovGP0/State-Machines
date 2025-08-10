using ActiveStateMachine.States;

namespace ActiveStateMachine.Messages.Replies;

public sealed record StateHistoryReply(string Source, string Target, IReadOnlyList<State> StateHistory)
    : StateMachineReply(new Version(1,0), "state-history", Source, Target, "");