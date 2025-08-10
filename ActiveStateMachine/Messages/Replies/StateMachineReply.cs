namespace ActiveStateMachine.Messages.Replies;

public abstract record StateMachineReply(Version Version, string Name, string Source, string Target, string MessageInfo)
    : StateMachineMessage(Version, Name, Source, Target, MessageInfo);