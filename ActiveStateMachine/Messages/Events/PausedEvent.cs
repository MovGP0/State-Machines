namespace ActiveStateMachine.Messages.Events;

public sealed record PausedEvent(string Source, string Target = "", string MessageInfo = "")
    : StateMachineEvent(new Version(1,0), "paused", Source, Target, MessageInfo);