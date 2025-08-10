namespace ActiveStateMachine.Messages.Events;

public sealed record ErrorEvent(string Source, string MessageInfo, Exception Exception)
    : StateMachineEvent(new Version(1,0), "error", Source, "", MessageInfo);