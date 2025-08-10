using Microsoft.Extensions.Logging;

namespace ActiveStateMachine.Internals;

internal static partial class StateMachineLog
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Trace, Message = "Preconditions for transition '{TransitionName}' are met.")]
    public static partial void PreconditionsMet(this ILogger logger, string transitionName);

    [LoggerMessage(EventId = 1001, Level = LogLevel.Warning, Message = "Transition '{TransitionName}' rejected: wrong current state '{CurrentState}', expected source '{ExpectedSource}'.")]
    public static partial void TransitionWrongState(this ILogger logger, string transitionName, string currentState, string expectedSource);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Warning, Message = "Transition '{TransitionName}' rejected: precondition '{PreconditionName}' not met.")]
    public static partial void PreconditionFailed(this ILogger logger, string transitionName, string preconditionName);

    [LoggerMessage(EventId = 1003, Level = LogLevel.Information, Message = "Transition '{TransitionName}' from '{SourceState}' to '{TargetState}'.")]
    public static partial void TransitionExecuting(this ILogger logger, string transitionName, string sourceState, string targetState);

    [LoggerMessage(EventId = 1004, Level = LogLevel.Warning, Message = "Could not {Operation} because current state is {CurrentState}.")]
    public static partial void InvalidLifecycleOperation(this ILogger logger, string operation, StateMachineState currentState);

    [LoggerMessage(EventId = 1005, Level = LogLevel.Error, Message = "Unhandled exception in worker loop.")]
    public static partial void WorkerException(this ILogger logger, Exception exception);

    [LoggerMessage(EventId = 1006, Level = LogLevel.Debug, Message = "Trigger '{TriggerName}' enqueued.")]
    public static partial void TriggerEnqueued(this ILogger logger, string triggerName);
}