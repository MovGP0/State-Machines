namespace ActiveStateMachine.Messages.Commands;

public sealed record FireTriggerCommand(string Source, string Target, string TriggerName, string MessageInfo = "")
    : StateMachineCommand(new Version(1,0), "fire", Source, Target, MessageInfo);