using ActiveStateMachine.Builder;

namespace ActiveStateMachine.Tests;

/// <summary>
/// Contains unit tests for the StateBuilder class.
/// </summary>
public static class StateBuilderTests
{
    public sealed class BuilderMethodTests
    {
        [Fact]
        public void ShouldReturnSameInstanceWhenAsTerminal()
        {
            // Arrange
            var sb = new StateMachineBuilder("machine").State("A");

            // Act
            var result = sb.AsTerminal();

            // Assert
            result.ShouldBe(sb);
        }

        [Fact]
        public void ShouldReturnSameInstanceWhenOnEnterAndOnExit()
        {
            // Arrange
            var sb = new StateMachineBuilder("machine").State("A");

            // Act
            var sbAfterEnter = sb.OnEnter("enter", () => { });
            var sbAfterExit = sb.OnExit("exit", () => { });

            // Assert
            sbAfterEnter.ShouldBe(sb);
            sbAfterExit.ShouldBe(sb);
        }

        [Fact]
        public void ShouldReturnTransitionBuilderOnOn()
        {
            // Arrange
            var sb = new StateMachineBuilder("machine").State("A");

            // Act
            var tb = sb.On("trigger");

            // Assert
            tb.ShouldNotBeNull();
            tb.ShouldBeOfType<TransitionBuilder>();
        }
    }
}