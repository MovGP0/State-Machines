namespace ActiveStateMachine.Messages.Requests;

public abstract record StateMachineRequest(Version Version, string Name, string Source, string Target, string MessageInfo)
    : StateMachineMessage(Version, Name, Source, Target, MessageInfo);