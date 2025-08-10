# ActiveStateMachine — Modern C# State Machine Framework

**ActiveStateMachine** is a modern, strongly‑typed, asynchronous state machine framework for .NET 9+.

It lets you model finite state machines with clear semantics for states, triggers, transitions and side‑effects.

The library separates the declaration of your state machine from its execution.

## 🚀 Quick Start

State machines are declared using a fluent builder API, but can also be created directly when more control is required.

You start with a `StateMachineBuilder` (or the generic `StateMachineBuilder<TTrigger>` for strongly typed triggers)
and add one or more states. Exactly one state must be marked as the default state.
Each state can define entry/exit actions and transitions in response to triggers.
Transitions can also declare preconditions and transition actions.

```csharp
using ActiveStateMachine;
using Microsoft.Extensions.Logging;

// Create a builder for a machine named "OrderFlow".
var builder = new StateMachineBuilder("OrderFlow");

// Define the default state with an entry action and a transition.
builder.State("Pending", isDefault: true)
    .OnEnter("LogPending", () => Console.WriteLine("Entering pending"))
    .On("Submit")
        .Do("LogSubmit", () => Console.WriteLine("Order submitted"))
        .GoTo("Processing");

// Define another state with an exit action and a transition back.
builder.State("Processing")
    .OnExit("LogProcessingExit", () => Console.WriteLine("Leaving processing"))
    .On("Complete").GoTo("Pending");

// Build the state definitions.
var states = builder.Build();

// Create a logger; you can use ILogger from any logging framework.
using var loggerFactory = LoggerFactory.Create(b => { });
var logger = loggerFactory.CreateLogger("OrderFlow");

// Create an active state machine with a bounded trigger queue of capacity 10.
var sm = new StateMachine("OrderFlow", states, queueCapacity: 10, logger);

// Start the worker thread and process triggers.
sm.Start();

// Fire some triggers; triggers are queued and processed asynchronously.
await sm.FireAsync("Submit");
await sm.FireAsync("Complete");

// Stop the worker when finished.
await sm.StopAsync();
```

### 🐚 Generic Triggers

If your triggers are an enum or some other type, use the generic variant:

```csharp
enum Trigger { Submit, Complete }

var builder = new StateMachineBuilder<Trigger>("Orders");
builder.State("Pending", isDefault: true).On(Trigger.Submit).GoTo("Processing");
builder.State("Processing").On(Trigger.Complete).GoTo("Pending");

var states = builder.Build();
var logger = LoggerFactory.Create(b => { }).CreateLogger("Orders");
var sm = new StateMachine<Trigger>("Orders", states, queueCapacity: 5, logger);
sm.Start();
await sm.FireAsync(Trigger.Submit);
await sm.FireAsync(Trigger.Complete);
await sm.StopAsync();
```

## ⏱ Lifecycle control

`StateMachine` instances expose lifecycle methods to control the worker thread:

| Method | Description |
|--------|-------------|
| `Start()` | Begin processing triggers. The machine transitions from Initialized to Running and publishes a StartedEvent. |
| `Pause()` | temporarily halt trigger processing. Transitions resume when you call `Resume()`. |
| `Resume()` | continue processing after a pause. |
| `StopAsync()` | gracefully stop the worker. Once stopped, no further triggers are processed. |

----

You can also control and query the machine via messages sent through the `Send()` method.

A `StateMachine` supports the following messages:

- **Commands:**
  - `StartCommand`
  - `PauseCommand`
  - `ResumeCommand`
  - `StopCommand`
  - `FireTriggerCommand`
- **Request / Replies:**
  - `GetStateRequest` / `StateReply`
  - `GetStateHistoryRequest` / `StateHistoryReply`
  - `GetPossibleStatesRequest` / `PossibleStatesReply`
  - `GetMessageHistoryRequest` / `MessageHistoryReply`.

### 💡 Consuming the async event stream

Messages can be consumed by subscribing observers (`IObserver<StateMachineMessage>`).

Every event and reply is recorded in `MessageHistory` and streamed to subscribers.

Use this to build reactive pipelines or to inspect machine behaviour.

Every `ActiveStateMachine` exposes:
```csharp
IAsyncEnumerable<StateMachineMessage> GetMessagesAsync(CancellationToken token = default);
```

You can subscribe to state change events, action executions, and more:

```csharp
var cts = new CancellationTokenSource();
_ = Task.Run(async () =>
{
    await foreach (var msg in sm.GetMessagesAsync(cts.Token))
    {
        Console.WriteLine($"[{msg.Timestamp}] {msg}");
    }
});

// Fire some triggers
sm.Fire("Submit");
sm.Fire("StartProcessing");
sm.Fire("Finish");
```

## 📈 Exporting to GraphML

To visualise your state machine, use StateMachineGraphExporter:

```csharp
var states = builder.Build();
var graph = StateMachineGraphExporter.ExportGraphMl("OrderFlow", states);
graph.Save("orderflow.graphml");
```

## 🔍 Analyzer Rules

The `ActiveStateMachine.Analyzers` project provides compile‑time diagnostics for common mistakes:

| ID     | Title	                                    | Severity     | Description                                                    |
|--------|-------------------------------------------|--------------|----------------------------------------------------------------|
| ASM001 | No default state declared	                | ❌&nbsp;Error | 	At least one state must be marked `IsDefault: true`.            |
| ASM002 | Multiple default states declared	         | ❌&nbsp;Error      | 	Only one state can be default.                                |
| ASM003 | Duplicate state name	                     | ❌&nbsp;Error      | 	A state is declared more than once.                           |
| ASM004 | Missing target state	                     | ❌&nbsp;Error      | 	A transition points to an undefined state.                    |
| ASM005 | Duplicate trigger in state	               | ❌&nbsp;Error      | 	The same trigger is defined multiple times in the same state. |
| ASM006 | Unreachable state	                        | ⚠️&nbsp;Warning   | 	State is never reached from the default state.                |
| ASM007 | No outgoing transitions and not terminal	 | ℹ️&nbsp;Info      | 	State has no transitions and is not marked `.AsTerminal()`.     |

## 🛠 Code Fixes

The `ActiveStateMachine.CodeFixes` project includes code fixes to resolve some diagnostics automatically.

| ID | Description   |
|----|---------------|
| ASM001 | Mark first state as default. |
| ASM004 | Add missing target state. |
| ASM005 | Remove duplicate trigger chain. |

## 🧩 Builder API

```csharp
builder.State("Name", isDefault: false)
       .AsTerminal()
       .OnEnter("ActionName", () => { /* entry action */ })
       .OnExit("ActionName", () => { /* exit action */ })
       .On("TriggerName")
           .When("ConditionName", () => true)
           .Do("ActionName", () => { /* action */ })
           .GoTo("TargetState");
```

🧪 Testing
Unit tests for this library can be found in the ActiveStateMachine.Tests project. Tests use xUnit and Shouldly to verify the behaviour of transitions, builders, the state machine lifecycle and the message API.

## 📄 License

This code is licensed under the [MIT License](./LICENSE).
