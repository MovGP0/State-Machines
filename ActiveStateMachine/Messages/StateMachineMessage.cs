namespace ActiveStateMachine.Messages;

public abstract record StateMachineMessage(
    Version Version,
    string Name,
    string Source,
    string Target,
    string MessageInfo,
    DateTime TimestampUtc,
    Guid Id)
{
    protected StateMachineMessage(
        Version version,
        string name,
        string source,
        string target,
        string messageInfo)
        : this(version, name, source, target, messageInfo, DateTime.UtcNow, Guid.NewGuid()) { }
}