using ActiveStateMachine.Transitions;

namespace ActiveStateMachine.Tests;

/// <summary>
/// Contains unit tests for the TransitionAction class.
/// </summary>
public static class TransitionActionTests
{
    public sealed class ConstructorTests
    {
        [Fact]
        public void ShouldCreateTransitionActionWithNameAndAction()
        {
            // Arrange
            var called = false;
            Action action = () => called = true;

            // Act
            var ta = new TransitionAction("test-action", action);

            // Assert
            ta.ShouldSatisfyAllConditions(
                () => ta.Name.ShouldBe("test-action"),
                () => ta.ShouldNotBeNull()
            );
            // Execute to ensure the provided action is invoked
            ta.Execute();
            called.ShouldBeTrue();
        }

        [Fact]
        public void ShouldThrowIfNameIsNull()
        {
            // Arrange
            Action action = () => { };

            // Act & Assert
            Should.Throw<ArgumentNullException>(() => new TransitionAction(null!, action));
        }

        [Fact]
        public void ShouldThrowIfActionIsNull()
        {
            // Arrange, Act & Assert
            Should.Throw<ArgumentNullException>(() => new TransitionAction("name", null!));
        }
    }
}