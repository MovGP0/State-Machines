namespace ActiveStateMachine.Messages.Requests;

public sealed record GetMessageHistoryRequest(string Source, string Target, int? MaximumCount = null)
    : StateMachineRequest(new Version(1,0), "get-message-history", Source, Target, "");