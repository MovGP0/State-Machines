using ActiveStateMachine.States;

namespace ActiveStateMachine.Messages.Replies;

public sealed record StateHistoryReply<TTrigger>(string Source, string Target, IReadOnlyList<State<TTrigger>> StateHistory)
    : StateMachineReply(new Version(1,0), "state-history", Source, Target, "");