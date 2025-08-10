namespace ActiveStateMachine.Messages.Events;

public sealed record StoppedEvent(string Source, string Target = "", string MessageInfo = "")
    : StateMachineEvent(new Version(1,0), "stopped", Source, Target, MessageInfo);