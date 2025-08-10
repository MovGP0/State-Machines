namespace ActiveStateMachine.Analyzers;

internal sealed record StateInfo(
    string Name,
    bool IsDefault,
    bool IsTerminal,
    List<(string Trigger, Location Where)> TriggerOccurrences,
    int OutgoingCount
);