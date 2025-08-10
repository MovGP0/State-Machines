namespace ActiveStateMachine.Messages.Events;

public abstract record StateMachineEvent(Version Version, string Name, string Source, string Target, string MessageInfo)
    : StateMachineMessage(Version, Name, Source, Target, MessageInfo);