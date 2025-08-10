namespace ActiveStateMachine.Messages.Commands;

public sealed record StartCommand(string Source, string Target, string MessageInfo = "")
    : StateMachineCommand(new Version(1,0), "start", Source, Target, MessageInfo);