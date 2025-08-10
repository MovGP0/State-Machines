namespace ActiveStateMachine.Analyzers;

public static class Descriptors
{
    public static readonly DiagnosticDescriptor NoDefaultState =
        new(id: "ASM001",
            title: "No default state declared",
            messageFormat: "State machine builder '{0}' declares no default state.",
            category: "StateMachine",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MultipleDefaultStates =
        new(id: "ASM002",
            title: "Multiple default states declared",
            messageFormat: "State machine builder '{0}' declares multiple default states: {1}.",
            category: "StateMachine",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateStates =
        new(id: "ASM003",
            title: "Duplicate state name",
            messageFormat: "State '{0}' is declared multiple times.",
            category: "StateMachine",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingTargetState =
        new(id: "ASM004",
            title: "Transition target state not found",
            messageFormat: "State '{0}' has transition to undefined target state '{1}'.",
            category: "StateMachine",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateTriggersInState =
        new("ASM005", "Duplicate trigger in state",
            "State '{0}' defines the trigger '{1}' multiple times.",
            "StateMachine", DiagnosticSeverity.Error, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnreachableState =
        new("ASM006", "Unreachable state",
            "State '{0}' is unreachable from the default state.",
            "StateMachine", DiagnosticSeverity.Warning, isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingOutgoingTransitionOrTerminal =
        new("ASM007", "State has no outgoing transitions",
            "State '{0}' has no outgoing transitions. Mark it as terminal with '.AsTerminal()' or add at least one transition.",
            "StateMachine", DiagnosticSeverity.Info, isEnabledByDefault: true);
}