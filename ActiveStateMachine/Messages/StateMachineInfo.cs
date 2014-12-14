namespace ActiveStateMachine.Messages
{
    [ToString]
    public sealed class StateMachineInfo
    {
        public StateMachineInfo(string name, string eventInfo, string source, string target)
        {
            Name = name;
            EventInfo = eventInfo;
            Source = source;
            Target = target;
        }

        public string Name { get; }
        public string EventInfo { get; }
        public string Source { get; }
        public string Target { get; }
    }
}