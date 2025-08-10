using ActiveStateMachine.Builder;

namespace ActiveStateMachine.Tests;

/// <summary>
/// Contains unit tests for the TransitionBuilder class.
/// </summary>
public static class TransitionBuilderTests
{
    public sealed class FluentMethodTests
    {
        [Fact]
        public void ShouldBuildTransitionAndReturnParentOnGoTo()
        {
            // Arrange
            var builder = new StateMachineBuilder("machine");
            var sbA = builder.State("A", isDefault: true);
            builder.State("B");

            // Act
            var returned = sbA.On("trigger").When("pre", () => true).Do("act", () => { }).GoTo("B");

            // Assert
            returned.ShouldBe(sbA);

            // Build and verify transition created
            var states = builder.Build();
            var stateA = states.Single(s => s.StateName == "A");
            stateA.TransitionList.Count.ShouldBe(1);
            var transition = stateA.TransitionList[0];
            transition.ShouldSatisfyAllConditions(
                () => transition.Trigger.ShouldBe("trigger"),
                () => transition.SourceStateName.ShouldBe("A"),
                () => transition.TargetStateName.ShouldBe("B"),
                () => transition.Preconditions.Count.ShouldBe(1),
                () => transition.TransitionActions.Count.ShouldBe(1)
            );
        }

        [Fact]
        public void ShouldThrowIfTargetIsNull()
        {
            // Arrange
            var builder = new StateMachineBuilder("machine");
            var sb = builder.State("A", isDefault: true);

            // Act & Assert
            Should.Throw<ArgumentNullException>(() => sb.On("t").GoTo(null!));
        }
    }
}