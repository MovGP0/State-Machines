namespace ActiveStateMachine.Messages.Events;

public sealed record CompletedEvent(string Source, string Target = "", string MessageInfo = "")
    : StateMachineEvent(new Version(1,0), "completed", Source, Target, MessageInfo);