using ActiveStateMachine.Messages;
using ActiveStateMachine.Messages.Events;

namespace ActiveStateMachine.Tests.Messages.Events;

public sealed class EventTests
{
    public static IEnumerable<object[]> EventData => new List<object[]>
    {
        new object[] { new InitializedEvent("s"), "initialized" },
        new object[] { new StartedEvent("s"), "started" },
        new object[] { new PausedEvent("s"), "paused" },
        new object[] { new ResumedEvent("s"), "resumed" },
        new object[] { new StoppedEvent("s"), "stopped" },
        new object[] { new CompletedEvent("s"), "completed" },
        new object[] { new InfoEvent("s", "info"), "info" },
        new object[] { new ErrorEvent("s", "error", new Exception()), "error" }
    };

    [Theory]
    [MemberData(nameof(EventData))]
    public void ShouldHaveCorrectEventName(StateMachineMessage evt, string expectedName)
    {
        // Assert
        evt.Name.ShouldBe(expectedName);
    }
}