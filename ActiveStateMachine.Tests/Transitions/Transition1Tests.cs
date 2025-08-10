using ActiveStateMachine.Transitions;

namespace ActiveStateMachine.Tests;

/// <summary>
/// Contains unit tests for the generic Transition class.
/// </summary>
public static class Transition1Tests
{
    public sealed class ConstructorTests
    {
        [Fact]
        public void ShouldInitializePropertiesForGenericTransition()
        {
            // Arrange
            var pc = new List<TransitionPrecondition> { new("pc", () => true) };
            var actions = new List<TransitionAction> { new("do", () => { }) };

            // Act
            var transition = new Transition<int>(
                name: "tr",
                trigger: 42,
                sourceStateName: "X",
                targetStateName: "Y",
                preconditions: pc,
                transitionActions: actions);

            // Assert
            transition.ShouldSatisfyAllConditions(
                () => transition.Name.ShouldBe("tr"),
                () => transition.Trigger.ShouldBe(42),
                () => transition.SourceStateName.ShouldBe("X"),
                () => transition.TargetStateName.ShouldBe("Y"),
                () => transition.Preconditions.ShouldBe(pc),
                () => transition.TransitionActions.ShouldBe(actions)
            );
        }

        [Fact]
        public void ShouldAllowChangingNameForGenericTransition()
        {
            // Arrange
            var transition = new Transition<string>("old", "trg", "S", "T", [], []);

            // Act
            transition.Name = "new";

            // Assert
            transition.Name.ShouldBe("new");
        }
    }
}