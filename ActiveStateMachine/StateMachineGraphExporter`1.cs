using System.Xml.Linq;
using ActiveStateMachine.States;

namespace ActiveStateMachine;

public static partial class StateMachineGraphExporter
{
    public static XDocument ExportGraphMl<TTrigger>(string machineName, IEnumerable<State<TTrigger>> states)
    {
        var stateList = states.ToList();
        var namespaceGraphMl = XNamespace.Get("http://graphml.graphdrawing.org/xmlns");
        var namespaceY = XNamespace.Get("http://www.yworks.com/xml/graphml");

        var graphElement = new XElement(namespaceGraphMl + "graph",
            new XAttribute("id", machineName),
            new XAttribute("edgedefault", "directed"));

        foreach (var state in stateList)
        {
            var node = new XElement(namespaceGraphMl + "node",
                new XAttribute("id", state.StateName));

            // Minimal yFiles node styling so yEd renders nicely
            node.Add(new XElement(namespaceGraphMl + "data",
                new XAttribute("key", "d0"),
                new XElement(namespaceY + "ShapeNode",
                    new XElement(namespaceY + "NodeLabel", state.StateName),
                    new XElement(namespaceY + "Shape", new XAttribute("type", state.IsDefaultState ? "roundrectangle" : "rectangle")))));

            graphElement.Add(node);
        }

        foreach (var state in stateList)
        {
            foreach (var transition in state.TransitionList)
            {
                var edge = new XElement(namespaceGraphMl + "edge",
                    new XAttribute("id", $"{transition.Name}"),
                    new XAttribute("source", transition.SourceStateName),
                    new XAttribute("target", transition.TargetStateName));

                edge.Add(new XElement(namespaceGraphMl + "data",
                    new XAttribute("key", "d1"),
                    new XElement(namespaceY + "PolyLineEdge",
                        new XElement(namespaceY + "EdgeLabel", transition.Name))));

                graphElement.Add(edge);
            }
        }

        return new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            new XElement(namespaceGraphMl + "graphml",
                // declare yEd keys (minimal)
                new XElement(namespaceGraphMl + "key",
                    new XAttribute("id", "d0"),
                    new XAttribute("for", "node"),
                    new XAttribute("yfiles.type", "nodegraphics")),
                new XElement(namespaceGraphMl + "key",
                    new XAttribute("id", "d1"),
                    new XAttribute("for", "edge"),
                    new XAttribute("yfiles.type", "edgegraphics")),
                graphElement));
    }

    public static void SaveGraphMl<TTrigger>(string machineName, IEnumerable<State<TTrigger>> states, string filePath)
        => ExportGraphMl(machineName, states).Save(filePath);
}