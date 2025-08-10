using ActiveStateMachine.States;

namespace ActiveStateMachine.Messages.Replies;

public sealed record PossibleStatesReply<TTrigger>(string Source, string Target, IReadOnlyList<State<TTrigger>> PossibleStates)
    : StateMachineReply(new Version(1,0), "possible-states", Source, Target, "");