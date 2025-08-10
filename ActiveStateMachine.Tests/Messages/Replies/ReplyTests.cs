using ActiveStateMachine.Builder;
using ActiveStateMachine.Messages;
using ActiveStateMachine.Messages.Events;
using ActiveStateMachine.Messages.Replies;
using ActiveStateMachine.States;
using ActiveStateMachine.Transitions;

namespace ActiveStateMachine.Tests.Messages.Replies;

public sealed class ReplyTests
{
    [Fact]
    public void ShouldCreateStateReply()
    {
        // Arrange
        var reply = new StateReply("src", "tgt", StateMachineState.Running);
        // Assert
        reply.ShouldSatisfyAllConditions(
            () => reply.Name.ShouldBe("state"),
            () => reply.CurrentState.ShouldBe(StateMachineState.Running),
            () => reply.Source.ShouldBe("src"),
            () => reply.Target.ShouldBe("tgt")
        );
    }

    [Fact]
    public void ShouldCreateStateHistoryReply()
    {
        // Arrange
        var states = new List<State> { new SimpleState("A", new List<Transition>(), null, null) };
        var reply = new StateHistoryReply("src", "tgt", states);
        // Assert
        reply.ShouldSatisfyAllConditions(
            () => reply.Name.ShouldBe("state-history"),
            () => reply.StateHistory.ShouldBe(states)
        );
    }

    [Fact]
    public void ShouldCreatePossibleStatesReply()
    {
        // Arrange
        var states = new List<State> { new SimpleState("A", new List<Transition>(), null, null) };
        var reply = new PossibleStatesReply("src", "tgt", states);
        // Assert
        reply.ShouldSatisfyAllConditions(
            () => reply.Name.ShouldBe("possible-states"),
            () => reply.PossibleStates.ShouldBe(states)
        );
    }

    [Fact]
    public void ShouldCreateMessageHistoryReply()
    {
        // Arrange
        var messages = new List<StateMachineMessage> { new InitializedEvent("s") };
        var reply = new MessageHistoryReply("src", "tgt", messages);
        // Assert
        reply.ShouldSatisfyAllConditions(
            () => reply.Name.ShouldBe("message-history"),
            () => reply.Messages.ShouldBe(messages)
        );
    }
}