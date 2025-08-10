namespace ActiveStateMachine.Messages.Replies;

public sealed record MessageHistoryReply(string Source, string Target, IReadOnlyList<StateMachineMessage> Messages)
    : StateMachineReply(new Version(1,0), "message-history", Source, Target, "");