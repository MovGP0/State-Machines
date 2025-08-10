namespace ActiveStateMachine.Messages.Events;

public sealed record InitializedEvent(string Source, string Target = "", string MessageInfo = "")
    : StateMachineEvent(new Version(1,0), "initialized", Source, Target, MessageInfo);