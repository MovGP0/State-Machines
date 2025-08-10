using ActiveStateMachine.Builder;

namespace ActiveStateMachine.Tests;

/// <summary>
/// Contains unit tests for the StateMachineBuilder class.
/// </summary>
public static class StateMachineBuilderTests
{
    public sealed class ConstructionTests
    {
        [Fact]
        public void ShouldThrowIfMachineNameIsNullOrWhitespace()
        {
            // Arrange, Act & Assert
            Should.Throw<ArgumentException>(() => new StateMachineBuilder(""));
            Should.Throw<ArgumentException>(() => new StateMachineBuilder("   "));
        }

        [Fact]
        public void ShouldRequireExactlyOneDefaultState()
        {
            // Arrange
            var builder = new StateMachineBuilder("machine");
            builder.State("A");
            builder.State("B");

            // Act & Assert
            Should.Throw<InvalidOperationException>(() => builder.Build());
        }

        [Fact]
        public void ShouldThrowWhenMultipleDefaultsAreDefined()
        {
            // Arrange
            var builder = new StateMachineBuilder("machine");
            builder.State("A", isDefault: true);

            // Act
            var ex = Should.Throw<InvalidOperationException>(() => builder.State("B", isDefault: true));
            ex.Message.ShouldContain("Default state already");
        }

        [Fact]
        public void ShouldThrowIfTransitionTargetsUnknownState()
        {
            // Arrange
            var builder = new StateMachineBuilder("machine");
            var sb = builder.State("A", isDefault: true);
            sb.On("t").GoTo("Unknown");

            // Act & Assert
            Should.Throw<InvalidOperationException>(() => builder.Build());
        }
    }

    public sealed class BuildTests
    {
        [Fact]
        public void ShouldBuildStatesWithTransitions()
        {
            // Arrange
            var builder = new StateMachineBuilder("machine");
            var a = builder.State("A", isDefault: true);
            builder.State("B");
            a.On("t").GoTo("B");

            // Act
            var states = builder.Build();

            // Assert
            states.Count.ShouldBe(2);
            var stateA = states.Single(s => s.StateName == "A");
            stateA.TransitionList.Count.ShouldBe(1);
            var transition = stateA.TransitionList[0];
            transition.ShouldSatisfyAllConditions(
                () => transition.Trigger.ShouldBe("t"),
                () => transition.SourceStateName.ShouldBe("A"),
                () => transition.TargetStateName.ShouldBe("B")
            );
        }
    }
}