namespace ActiveStateMachine.Messages.Commands;

public sealed record StopCommand(string Source, string Target, string MessageInfo = "")
    : StateMachineCommand(new Version(1,0), "stop", Source, Target, MessageInfo);