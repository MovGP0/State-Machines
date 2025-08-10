namespace ActiveStateMachine.Messages.Requests;

public sealed record GetStateRequest(string Source, string Target)
    : StateMachineRequest(new Version(1,0), "get-state", Source, Target, "");