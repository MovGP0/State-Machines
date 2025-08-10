using ActiveStateMachine.Messages;
using ActiveStateMachine.Messages.Commands;

namespace ActiveStateMachine.Tests.Messages.Commands;

public sealed class CommandTests
{
    public static IEnumerable<object[]> CommandData => new List<object[]>
    {
        new object[] { new StartCommand("s","t"), "start" },
        new object[] { new PauseCommand("s","t"), "pause" },
        new object[] { new ResumeCommand("s","t"), "resume" },
        new object[] { new StopCommand("s","t"), "stop" },
        new object[] { new FireTriggerCommand("s","t","x"), "fire" }
    };

    [Theory]
    [MemberData(nameof(CommandData))]
    public void ShouldHaveCorrectCommandName(StateMachineMessage command, string expectedName)
    {
        // Assert
        command.Name.ShouldBe(expectedName);
    }
}