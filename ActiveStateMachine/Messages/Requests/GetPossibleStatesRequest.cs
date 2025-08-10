namespace ActiveStateMachine.Messages.Requests;

public sealed record GetPossibleStatesRequest(string Source, string Target)
    : StateMachineRequest(new Version(1,0), "get-possible-states", Source, Target, "");