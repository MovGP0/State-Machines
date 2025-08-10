using ActiveStateMachine.Transitions;

namespace ActiveStateMachine.Tests;

/// <summary>
/// Contains unit tests for the TransitionPrecondition class.
/// </summary>
public static class TransitionPreconditionTests
{
    public sealed class ConstructorTests
    {
        [Fact]
        public void ShouldCreatePreconditionWithNameAndPredicate()
        {
            // Arrange
            var valid = true;
            Func<bool> predicate = () => valid;

            // Act
            var precondition = new TransitionPrecondition("is-valid", predicate);

            // Assert
            precondition.ShouldSatisfyAllConditions(
                () => precondition.Name.ShouldBe("is-valid"),
                () => precondition.IsValid.ShouldBeTrue()
            );
        }

        [Fact]
        public void ShouldReflectPredicateResult()
        {
            // Arrange
            var valid = false;
            var precondition = new TransitionPrecondition("pc", () => valid);

            // Act & Assert
            precondition.IsValid.ShouldBeFalse();
        }

        [Fact]
        public void ShouldThrowIfNameIsNull()
        {
            // Arrange
            Func<bool> predicate = () => true;

            // Act & Assert
            Should.Throw<ArgumentNullException>(() => new TransitionPrecondition(null!, predicate));
        }

        [Fact]
        public void ShouldThrowIfPredicateIsNull()
        {
            // Act & Assert
            Should.Throw<ArgumentNullException>(() => new TransitionPrecondition("name", null!));
        }
    }
}