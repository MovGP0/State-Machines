namespace ActiveStateMachine.Messages.Replies;

public sealed record StateReply(string Source, string Target, StateMachineState CurrentState)
    : StateMachineReply(new Version(1,0), "state", Source, Target, "");