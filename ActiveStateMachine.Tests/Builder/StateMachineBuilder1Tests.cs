using ActiveStateMachine.Builder;

namespace ActiveStateMachine.Tests;

/// <summary>
/// Contains unit tests for the generic StateMachineBuilder class.
/// </summary>
public static class StateMachineBuilder1Tests
{
    public sealed class ConstructionTests
    {
        [Fact]
        public void ShouldThrowIfMachineNameIsNullOrWhitespaceGeneric()
        {
            // Arrange, Act & Assert
            Should.Throw<ArgumentException>(() => new StateMachineBuilder<int>(""));
            Should.Throw<ArgumentException>(() => new StateMachineBuilder<int>("   "));
        }

        [Fact]
        public void ShouldRequireExactlyOneDefaultStateGeneric()
        {
            // Arrange
            var builder = new StateMachineBuilder<int>("machine");
            builder.State("A");
            builder.State("B");

            // Act & Assert
            Should.Throw<InvalidOperationException>(() => builder.Build());
        }

        [Fact]
        public void ShouldThrowWhenMultipleDefaultsAreDefinedGeneric()
        {
            // Arrange
            var builder = new StateMachineBuilder<string>("machine");
            builder.State("A", isDefault: true);

            // Act
            var ex = Should.Throw<InvalidOperationException>(() => builder.State("B", isDefault: true));
            ex.Message.ShouldContain("Default state already");
        }

        [Fact]
        public void ShouldThrowIfTransitionTargetsUnknownStateGeneric()
        {
            // Arrange
            var builder = new StateMachineBuilder<int>("machine");
            var sb = builder.State("A", isDefault: true);
            sb.On(1).GoTo("Unknown");

            // Act & Assert
            Should.Throw<InvalidOperationException>(() => builder.Build());
        }
    }

    public sealed class BuildTests
    {
        [Fact]
        public void ShouldBuildStatesWithTransitionsGeneric()
        {
            // Arrange
            var builder = new StateMachineBuilder<int>("machine");
            var a = builder.State("A", isDefault: true);
            builder.State("B");
            a.On(42).GoTo("B");

            // Act
            var states = builder.Build();

            // Assert
            states.Count.ShouldBe(2);
            var stateA = states.Single(s => s.StateName == "A");
            stateA.TransitionList.Count.ShouldBe(1);
            var transition = stateA.TransitionList[0];
            transition.ShouldSatisfyAllConditions(
                () => transition.Trigger.ShouldBe(42),
                () => transition.SourceStateName.ShouldBe("A"),
                () => transition.TargetStateName.ShouldBe("B")
            );
        }
    }
}