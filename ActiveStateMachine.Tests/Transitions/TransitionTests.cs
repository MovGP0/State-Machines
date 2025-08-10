using ActiveStateMachine.Transitions;

namespace ActiveStateMachine.Tests;

/// <summary>
/// Contains unit tests for the Transition class (non‑generic).
/// </summary>
public static class TransitionTests
{
    public sealed class ConstructorTests
    {
        [Fact]
        public void ShouldInitializeProperties()
        {
            // Arrange
            var pc = new List<TransitionPrecondition> { new("pc", () => true) };
            var actions = new List<TransitionAction> { new("do", () => { }) };

            // Act
            var transition = new Transition(
                name: "tr",
                trigger: "trigger",
                sourceStateName: "A",
                targetStateName: "B",
                preconditions: pc,
                transitionActions: actions);

            // Assert
            transition.ShouldSatisfyAllConditions(
                () => transition.Name.ShouldBe("tr"),
                () => transition.Trigger.ShouldBe("trigger"),
                () => transition.SourceStateName.ShouldBe("A"),
                () => transition.TargetStateName.ShouldBe("B"),
                () => transition.Preconditions.ShouldBe(pc),
                () => transition.TransitionActions.ShouldBe(actions)
            );
        }

        [Fact]
        public void ShouldAllowChangingName()
        {
            // Arrange
            var transition = new Transition("old", "t", "A", "B", [], []);

            // Act
            transition.Name = "new";

            // Assert
            transition.Name.ShouldBe("new");
        }
    }
}