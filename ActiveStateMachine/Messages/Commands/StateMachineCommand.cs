namespace ActiveStateMachine.Messages.Commands;

public abstract record StateMachineCommand(Version Version, string Name, string Source, string Target, string MessageInfo)
    : StateMachineMessage(Version, Name, Source, Target, MessageInfo);