using ActiveStateMachine.Builder;
using ActiveStateMachine.Messages;
using ActiveStateMachine.Messages.Commands;
using ActiveStateMachine.Messages.Events;
using ActiveStateMachine.Messages.Replies;
using ActiveStateMachine.Messages.Requests;
using ActiveStateMachine.States;
using ActiveStateMachine.Transitions;
using Microsoft.Extensions.Logging;

namespace ActiveStateMachine.Tests;

/// <summary>
/// Contains unit tests for the StateMachine class (non‑generic).
/// </summary>
public static class StateMachineTests
{
    public sealed class ConstructorTests
    {
        [Fact]
        public void ShouldThrowIfNameIsNullOrWhitespace()
        {
            // Arrange
            var states = new List<State> {
                new SimpleState("A", new List<Transition>(), null, null, true)
            };
            var logger = LoggerFactory.Create(b => { }).CreateLogger("test");

            // Act & Assert
            Should.Throw<ArgumentException>(() => new StateMachine(null!, states, 10, logger));
            Should.Throw<ArgumentException>(() => new StateMachine("", states, 10, logger));
        }

        [Fact]
        public void ShouldThrowIfStatesNull()
        {
            // Arrange
            var logger = LoggerFactory.Create(b => { }).CreateLogger("test");

            // Act & Assert
            Should.Throw<ArgumentNullException>(() => new StateMachine("machine", null!, 10, logger));
        }

        [Fact]
        public void ShouldRequireExactlyOneDefaultState()
        {
            // Arrange
            var states = new List<State> { new SimpleState("A", new List<Transition>(), null, null, false) };
            var logger = LoggerFactory.Create(b => { }).CreateLogger("test");

            // Act & Assert
            Should.Throw<ArgumentException>(() => new StateMachine("machine", states, 10, logger));
        }

        [Fact]
        public void ShouldInitializeWithDefaultStateAndEmitInitializedEvent()
        {
            // Arrange
            var states = new List<State> {
                new SimpleState("A", new List<Transition>(), null, null, true),
                new SimpleState("B", new List<Transition>(), null, null, false)
            };
            var logger = LoggerFactory.Create(b => { }).CreateLogger("test");

            // Act
            var sm = new StateMachine("machine", states, 10, logger);

            // Assert
            sm.ShouldSatisfyAllConditions(
                () => sm.Name.ShouldBe("machine"),
                () => sm.CurrentState.ShouldBe(StateMachineState.Initialized),
                () => sm.StateHistory.Count.ShouldBe(1),
                () => sm.StateHistory[0].StateName.ShouldBe("A"),
                () => sm.MessageHistory.Count.ShouldBe(1),
                () => sm.MessageHistory[0].ShouldBeOfType<InitializedEvent>()
            );
        }
    }

    public sealed class LifecycleTests
    {
        private static ILogger CreateLogger() => LoggerFactory.Create(b => { }).CreateLogger("test");

        private static StateMachine CreateMachine()
        {
            var builder = new StateMachineBuilder("machine");
            var a = builder.State("A", isDefault: true);
            builder.State("B");
            return new StateMachine("machine", builder.Build(), 10, CreateLogger());
        }

        [Fact]
        public void ShouldStartPauseResumeAndStop()
        {
            // Arrange
            var sm = CreateMachine();

            // Act
            sm.Start();
            sm.Pause();
            sm.Resume();
            sm.StopAsync().GetAwaiter().GetResult();

            // Assert
            // Verify last lifecycle event is stopped and state is stopped
            sm.ShouldSatisfyAllConditions(
                () => sm.CurrentState.ShouldBe(StateMachineState.Stopped),
                () => sm.MessageHistory.OfType<StartedEvent>().Count().ShouldBe(1),
                () => sm.MessageHistory.OfType<PausedEvent>().Count().ShouldBe(1),
                () => sm.MessageHistory.OfType<ResumedEvent>().Count().ShouldBe(1),
                () => sm.MessageHistory.OfType<StoppedEvent>().Count().ShouldBe(1)
            );
        }
    }

    public sealed class SendTests
    {
        private static ILogger CreateLogger() => LoggerFactory.Create(b => { }).CreateLogger("test");

        private static StateMachine CreateMachine()
        {
            var builder = new StateMachineBuilder("machine");
            var a = builder.State("A", isDefault: true);
            builder.State("B");
            return new StateMachine("machine", builder.Build(), 10, CreateLogger());
        }

        [Fact]
        public void ShouldIgnoreMessagesForDifferentTarget()
        {
            // Arrange
            var sm = CreateMachine();
            var initialCount = sm.MessageHistory.Count;

            // Act
            sm.Send(new StartCommand("src", "other"));

            // Assert
            sm.MessageHistory.Count.ShouldBe(initialCount);
            sm.CurrentState.ShouldBe(StateMachineState.Initialized);
        }

        [Fact]
        public void ShouldHandleStartPauseResumeStopViaSend()
        {
            // Arrange
            var sm = CreateMachine();

            // Act
            sm.Send(new StartCommand("src", "machine"));
            sm.Send(new PauseCommand("src", "machine"));
            sm.Send(new ResumeCommand("src", "machine"));
            sm.Send(new StopCommand("src", "machine"));
            // Wait a moment for stop to complete
            sm.StopAsync().GetAwaiter().GetResult();

            // Assert
            sm.MessageHistory.ShouldContain(m => m is StartedEvent);
            sm.MessageHistory.ShouldContain(m => m is PausedEvent);
            sm.MessageHistory.ShouldContain(m => m is ResumedEvent);
            sm.MessageHistory.ShouldContain(m => m is StoppedEvent);
        }

        [Fact]
        public void ShouldHandleRequestsAndAddReplyMessages()
        {
            // Arrange
            var sm = CreateMachine();
            sm.Send(new StartCommand("src", "machine"));
            var beforeReplies = sm.MessageHistory.Count;

            // Act
            sm.Send(new GetStateRequest("user", "machine"));
            sm.Send(new GetStateHistoryRequest("user", "machine"));
            sm.Send(new GetPossibleStatesRequest("user", "machine"));
            sm.Send(new GetMessageHistoryRequest("user", "machine"));

            // Assert
            // Four reply messages should have been added
            sm.MessageHistory.Count.ShouldBeGreaterThanOrEqualTo(beforeReplies + 4);
            sm.MessageHistory.OfType<StateReply>().Count().ShouldBe(1);
            sm.MessageHistory.OfType<StateHistoryReply>().Count().ShouldBe(1);
            sm.MessageHistory.OfType<PossibleStatesReply>().Count().ShouldBe(1);
            sm.MessageHistory.OfType<MessageHistoryReply>().Count().ShouldBe(1);
        }

        [Fact]
        public void ShouldDeliverMessagesToObserversAndUnsubscribe()
        {
            // Arrange
            var sm = CreateMachine();
            var received = new List<StateMachineMessage>();
            var observer = new TestObserver(m => received.Add(m));

            var subscription = sm.Subscribe(observer);

            // Act
            sm.Start();
            sm.Pause();
            subscription.Dispose();
            sm.Resume(); // should not reach observer

            // Assert
            received.ShouldContain(m => m is StartedEvent);
            received.ShouldContain(m => m is PausedEvent);
            received.ShouldNotContain(m => m is ResumedEvent);
        }
    }

    private sealed class TestObserver : IObserver<StateMachineMessage>
    {
        private readonly Action<StateMachineMessage> _onNext;
        public TestObserver(Action<StateMachineMessage> onNext) => _onNext = onNext;
        public void OnCompleted() { }
        public void OnError(Exception error) { }
        public void OnNext(StateMachineMessage value) => _onNext(value);
    }
}