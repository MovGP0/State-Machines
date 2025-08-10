using ActiveStateMachine.Builder;
using ActiveStateMachine.Transitions;

namespace ActiveStateMachine.Tests.Builder;

/// <summary>
/// Contains unit tests for the SimpleState class.
/// </summary>
public static class SimpleStateTests
{
    public sealed class ConstructorTests
    {
        [Fact]
        public void ShouldInitializeSimpleState()
        {
            // Arrange
            var transitions = new List<Transition> {
                new Transition("tr", "trigger", "state", "state2", [], [])
            };
            var entryActions = new List<TransitionAction> { new("enter", () => { }) };
            var exitActions = new List<TransitionAction> { new("exit", () => { }) };

            // Act
            var state = new SimpleState("state", transitions, entryActions, exitActions, isDefaultState: true);

            // Assert
            state.ShouldSatisfyAllConditions(
                () => state.StateName.ShouldBe("state"),
                () => state.TransitionList.ShouldBe(transitions),
                () => state.EntryActions.ShouldBe(entryActions),
                () => state.ExitActions.ShouldBe(exitActions),
                () => state.IsDefaultState.ShouldBeTrue()
            );
        }
    }
}