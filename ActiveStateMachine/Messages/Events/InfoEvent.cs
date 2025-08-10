namespace ActiveStateMachine.Messages.Events;

public sealed record InfoEvent(string Source, string MessageInfo)
    : StateMachineEvent(new Version(1,0), "info", Source, "", MessageInfo);