using ActiveStateMachine.Builder;
using ActiveStateMachine.Transitions;

namespace ActiveStateMachine.Tests.Builder;

/// <summary>
/// Contains unit tests for the generic SimpleState class.
/// </summary>
public static class SimpleState1Tests
{
    public sealed class ConstructorTests
    {
        [Fact]
        public void ShouldInitializeGenericSimpleState()
        {
            // Arrange
            var transitions = new List<Transition<int>> {
                new Transition<int>("tr", 1, "state", "state2", [], [])
            };
            var entryActions = new List<TransitionAction> { new("enter", () => { }) };
            var exitActions = new List<TransitionAction> { new("exit", () => { }) };

            // Act
            var state = new SimpleState<int>("state", transitions, entryActions, exitActions, isDefaultState: false);

            // Assert
            state.ShouldSatisfyAllConditions(
                () => state.StateName.ShouldBe("state"),
                () => state.TransitionList.ShouldBe(transitions),
                () => state.EntryActions.ShouldBe(entryActions),
                () => state.ExitActions.ShouldBe(exitActions),
                () => state.IsDefaultState.ShouldBeFalse()
            );
        }
    }
}