namespace ActiveStateMachine.Messages.Events;

public sealed record ResumedEvent(string Source, string Target = "", string MessageInfo = "")
    : StateMachineEvent(new Version(1,0), "resumed", Source, Target, MessageInfo);