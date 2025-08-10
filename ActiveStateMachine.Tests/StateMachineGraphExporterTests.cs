using ActiveStateMachine.Builder;
using ActiveStateMachine.States;
using ActiveStateMachine.Transitions;

namespace ActiveStateMachine.Tests;

/// <summary>
/// Contains unit tests for the StateMachineGraphExporter class.
/// </summary>
public static class StateMachineGraphExporterTests
{
    public sealed class GraphExportTests
    {
        [Fact]
        public void ShouldExportGraphMl()
        {
            // Arrange
            var transitionsA = new List<Transition> {
                new Transition("edge1", "t1", "A", "B", [], [])
            };
            var transitionsB = new List<Transition>();
            var states = new List<State> {
                new SimpleState("A", transitionsA, null, null, true),
                new SimpleState("B", transitionsB, null, null, false)
            };

            // Act
            var doc = StateMachineGraphExporter.ExportGraphMl("machine", states);

            // Assert
            doc.ShouldNotBeNull();
            var xml = doc.ToString();
            xml.ShouldContain("<graphml");
            xml.ShouldContain("node id=\"A\"");
            xml.ShouldContain("node id=\"B\"");
            xml.ShouldContain("edge id=\"edge1\" source=\"A\" target=\"B\"");
        }

        [Fact]
        public void ShouldSaveGraphMlToFile()
        {
            // Arrange
            var transitions = new List<Transition> { };
            var states = new List<State> { new SimpleState("A", transitions, null, null, true) };
            var tempFile = System.IO.Path.GetTempFileName();

            try
            {
                // Act
                StateMachineGraphExporter.SaveGraphMl("machine", states, tempFile);

                // Assert
                System.IO.File.Exists(tempFile).ShouldBeTrue();
                var contents = System.IO.File.ReadAllText(tempFile);
                contents.ShouldContain("<graphml");
                contents.ShouldContain("node id=\"A\"");
            }
            finally
            {
                if (System.IO.File.Exists(tempFile)) System.IO.File.Delete(tempFile);
            }
        }
    }
}