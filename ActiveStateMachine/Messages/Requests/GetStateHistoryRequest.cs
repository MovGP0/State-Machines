namespace ActiveStateMachine.Messages.Requests;

public sealed record GetStateHistoryRequest(string Source, string Target)
    : StateMachineRequest(new Version(1,0), "get-state-history", Source, Target, "");