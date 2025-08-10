namespace ActiveStateMachine.Messages.Events;

public sealed record StartedEvent(string Source, string Target = "", string MessageInfo = "")
    : StateMachineEvent(new Version(1,0), "started", Source, Target, MessageInfo);