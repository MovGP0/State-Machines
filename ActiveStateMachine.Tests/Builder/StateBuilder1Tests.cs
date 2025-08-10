using ActiveStateMachine.Builder;

namespace ActiveStateMachine.Tests;

/// <summary>
/// Contains unit tests for the generic StateBuilder class.
/// </summary>
public static class StateBuilder1Tests
{
    public sealed class BuilderMethodTests
    {
        [Fact]
        public void ShouldReturnSameInstanceWhenAsTerminalGeneric()
        {
            // Arrange
            var sb = new StateMachineBuilder<int>("machine").State("A");

            // Act
            var result = sb.AsTerminal();

            // Assert
            result.ShouldBe(sb);
        }

        [Fact]
        public void ShouldReturnSameInstanceWhenOnEnterAndOnExitGeneric()
        {
            // Arrange
            var sb = new StateMachineBuilder<int>("machine").State("A");

            // Act
            var sbAfterEnter = sb.OnEnter("enter", () => { });
            var sbAfterExit = sb.OnExit("exit", () => { });

            // Assert
            sbAfterEnter.ShouldBe(sb);
            sbAfterExit.ShouldBe(sb);
        }

        [Fact]
        public void ShouldReturnTransitionBuilderOnGenericOn()
        {
            // Arrange
            var sb = new StateMachineBuilder<string>("machine").State("A");

            // Act
            var tb = sb.On("trigger");

            // Assert
            tb.ShouldNotBeNull();
            tb.ShouldBeOfType<TransitionBuilder<string>>();
        }
    }
}