namespace ActiveStateMachine.Messages.Commands;

public sealed record PauseCommand(string Source, string Target, string MessageInfo = "")
    : StateMachineCommand(new Version(1,0), "pause", Source, Target, MessageInfo);