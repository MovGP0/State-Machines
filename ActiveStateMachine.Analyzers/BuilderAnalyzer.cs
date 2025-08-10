namespace ActiveStateMachine.Analyzers;

/// <summary>
/// Analyzes fluent uses of StateMachineBuilder{T} (string states) and StateMachineBuilder{TTrigger,TStateEnum}
/// and validates states/targets/defaults/duplicates/unreachables.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BuilderAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            Descriptors.NoDefaultState,
            Descriptors.MultipleDefaultStates,
            Descriptors.DuplicateStates,
            Descriptors.MissingTargetState,
            Descriptors.DuplicateTriggersInState,
            Descriptors.UnreachableState,
            Descriptors.MissingOutgoingTransitionOrTerminal);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation) return;
        if (invocation.Expression is not MemberAccessExpressionSyntax maes) return;
        if (maes.Name.Identifier.ValueText != "Build") return;

        var receiverType = ResolveReceiverType(context.SemanticModel, maes.Expression);
        if (receiverType is null) return;
        if (receiverType.ContainingNamespace?.ToDisplayString() != "ActiveStateMachine.Builder") return;

        var isBuilder = receiverType.Name == "StateMachineBuilder";
        if (!isBuilder) return;

        var block = invocation.FirstAncestorOrSelf<BlockSyntax>();
        if (block is null) return;

        var receiverId = GetReceiverIdentifier(maes.Expression);
        if (receiverId is null) return;

        var states = new Dictionary<string, StateInfo>(StringComparer.Ordinal);
        var transitions = new List<(string Source, string Target, string Trigger)>();
        string? lastStateNameForChain = null;

        foreach (var expr in block.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            if (expr.Expression is not MemberAccessExpressionSyntax m) continue;
            if (!SameReceiver(m.Expression, receiverId)) continue;

            var method = m.Name.Identifier.ValueText;

            if (method == "State")
            {
                var stateName = ExtractStateName(context.SemanticModel, expr.ArgumentList.Arguments.FirstOrDefault()?.Expression);
                if (stateName is null) continue;

                var isDefault = false;
                if (expr.ArgumentList.Arguments.Count >= 2)
                {
                    var val = context.SemanticModel.GetConstantValue(expr.ArgumentList.Arguments[1].Expression);
                    isDefault = val.HasValue && val.Value is bool b && b;
                }

                if (states.TryGetValue(stateName, out var info))
                {
                    // Duplicate state diagnostic
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.DuplicateStates, expr.GetLocation(), stateName));
                }
                else
                {
                    states[stateName] = new StateInfo(
                        stateName, isDefault, IsTerminal: false,
                        TriggerOccurrences: new(), OutgoingCount: 0);
                }

                lastStateNameForChain = stateName;
            }
            else if (method == "AsTerminal" && lastStateNameForChain is not null)
            {
                if (states.TryGetValue(lastStateNameForChain, out var info))
                    states[lastStateNameForChain] = info with { IsTerminal = true };
            }
            else if (method == "On")
            {
                // Remember trigger occurrence for ASM005
                var trigger = ExtractTriggerLiteral(context.SemanticModel, expr.ArgumentList.Arguments.FirstOrDefault()?.Expression) ?? "<unknown>";
                var src = lastStateNameForChain ?? NearestPreviousStateName(context.SemanticModel, expr);
                if (src is not null && states.TryGetValue(src, out var info))
                {
                    info.TriggerOccurrences.Add((trigger, expr.GetLocation()));
                    states[src] = info;
                }
            }
            else if (method == "GoTo")
            {
                var targetName = ExtractStateName(context.SemanticModel, expr.ArgumentList.Arguments.FirstOrDefault()?.Expression);
                var src = lastStateNameForChain ?? NearestPreviousStateName(context.SemanticModel, expr);
                var trig = NearestPreviousTriggerLiteral(context.SemanticModel, expr) ?? "<unknown>";

                if (src is not null && targetName is not null)
                {
                    transitions.Add((src, targetName, trig));
                    if (states.TryGetValue(src, out var info))
                        states[src] = info with { OutgoingCount = info.OutgoingCount + 1 };
                }
            }
        }

        // Default state diagnostics
        var defaults = states.Values.Where(s => s.IsDefault).Select(s => s.Name).ToList();
        if (defaults.Count == 0)
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.NoDefaultState, invocation.GetLocation(), receiverType.ToDisplayString()));
        }
        else if (defaults.Count > 1)
        {
            context.ReportDiagnostic(Diagnostic.Create(Descriptors.MultipleDefaultStates, invocation.GetLocation(), receiverType.ToDisplayString(), string.Join(", ", defaults)));
        }

        // Missing targets
        var stateNames = states.Keys.ToHashSet(StringComparer.Ordinal);
        foreach (var (src, tgt, _) in transitions)
            if (!stateNames.Contains(tgt))
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.MissingTargetState, invocation.GetLocation(), src, tgt));

        // ASM005: duplicate triggers per source (report at the extra On(...) sites)
        foreach (var s in states.Values)
        {
            var byTrigger = s.TriggerOccurrences.GroupBy(t => t.Trigger, StringComparer.Ordinal);
            foreach (var g in byTrigger)
            {
                if (g.Count() > 1)
                {
                    // report duplicates except the first occurrence
                    foreach (var dup in g.Skip(1))
                        context.ReportDiagnostic(Diagnostic.Create(Descriptors.DuplicateTriggersInState, dup.Where, s.Name, g.Key));
                }
            }
        }

        // ASM006: unreachable states (BFS from default if exactly one)
        if (defaults.Count == 1)
        {
            var start = defaults[0];
            var graph = BuildAdjacency(stateNames, transitions);
            var reachable = Bfs(graph, start);
            foreach (var s in stateNames)
            {
                if (!reachable.Contains(s))
                    context.ReportDiagnostic(Diagnostic.Create(Descriptors.UnreachableState, invocation.GetLocation(), s));
            }
        }

        // NEW: ASM007 – states with zero outgoing transitions and not terminal
        foreach (var s in states.Values)
        {
            if (s.OutgoingCount == 0 && !s.IsTerminal)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptors.MissingOutgoingTransitionOrTerminal, invocation.GetLocation(), s.Name));
            }
        }
    }

    private static INamedTypeSymbol? ResolveReceiverType(SemanticModel model, ExpressionSyntax receiver)
    {
        var info = model.GetTypeInfo(receiver);
        return info.Type as INamedTypeSymbol;
    }

    private static string? GetReceiverIdentifier(ExpressionSyntax expr) =>
        expr switch
        {
            IdentifierNameSyntax id => id.Identifier.ValueText,
            MemberAccessExpressionSyntax maes => GetReceiverIdentifier(maes.Expression),
            _ => null
        };

    private static bool SameReceiver(ExpressionSyntax expr, string receiverId) =>
        GetReceiverIdentifier(expr) == receiverId;

    private static string? ExtractStateName(SemanticModel model, ExpressionSyntax? expr)
    {
        if (expr is null) return null;

        // "Name" literal
        if (expr is LiteralExpressionSyntax lit && lit.IsKind(SyntaxKind.StringLiteralExpression))
            return lit.Token.ValueText;

        // Enum literal (e.g., OrderState.Submitted)
        var constant = model.GetConstantValue(expr);
        if (constant is { HasValue: true, Value: not null })
            return constant.Value.ToString();

        var typeInfo = model.GetTypeInfo(expr).Type;
        if (typeInfo is { TypeKind: TypeKind.Enum })
            return expr.ToString(); // fallback

        // nameof(State)
        if (expr is InvocationExpressionSyntax { Expression: IdentifierNameSyntax { Identifier.ValueText: "nameof" } } inv)
        {
            var arg = inv.ArgumentList.Arguments.FirstOrDefault()?.Expression?.ToString();
            return arg;
        }

        return null;
    }

    private static string? ExtractTriggerLiteral(SemanticModel model, ExpressionSyntax? expr)
    {
        if (expr is null) return null;

        var constant = model.GetConstantValue(expr);
        if (constant is { HasValue: true, Value: not null })
            return constant.Value.ToString();

        if (expr is LiteralExpressionSyntax lit)
            return lit.Token.Value?.ToString();

        return null;
    }

    private static string? NearestPreviousStateName(SemanticModel model, SyntaxNode node)
        => node.AncestorsAndSelf()
               .OfType<BlockSyntax>()
               .SelectMany(b => b.DescendantNodes().OfType<InvocationExpressionSyntax>())
               .Reverse()
               .Select(inv => inv.Expression as MemberAccessExpressionSyntax)
               .Where(m => m is not null && m.Name.Identifier.ValueText == "State")
               .Select(m => (m!, ((InvocationExpressionSyntax)m!.Parent!).ArgumentList.Arguments.FirstOrDefault()?.Expression))
               .Where(tuple => tuple.Item2 is not null)
               .Select(tuple => ExtractStateName(model, tuple.Item2))
               .FirstOrDefault(s => s is not null);

    private static string? NearestPreviousTriggerLiteral(SemanticModel model, SyntaxNode node)
        => node.AncestorsAndSelf()
               .OfType<BlockSyntax>()
               .SelectMany(b => b.DescendantNodes().OfType<InvocationExpressionSyntax>())
               .Reverse()
               .Select(inv => inv.Expression as MemberAccessExpressionSyntax)
               .Where(m => m is not null && m.Name.Identifier.ValueText == "On")
               .Select(m => (m!, ((InvocationExpressionSyntax)m!.Parent!).ArgumentList.Arguments.FirstOrDefault()?.Expression))
               .Where(tuple => tuple.Item2 is not null)
               .Select(tuple => ExtractTriggerLiteral(model, tuple.Item2))
               .FirstOrDefault(s => s is not null);

    private static Dictionary<string, HashSet<string>> BuildAdjacency(HashSet<string> states, List<(string source, string target, string trigger)> transitions)
    {
        var g = states.ToDictionary(s => s, _ => new HashSet<string>(StringComparer.Ordinal), StringComparer.Ordinal);
        foreach (var (source, target, _) in transitions)
        {
            if (!g.ContainsKey(source)) g[source] = new(StringComparer.Ordinal);
            g[source].Add(target);
        }
        return g;
    }

    private static HashSet<string> Bfs(Dictionary<string, HashSet<string>> graph, string start)
    {
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var q = new Queue<string>();
        if (graph.ContainsKey(start))
        {
            visited.Add(start);
            q.Enqueue(start);
        }
        while (q.Count > 0)
        {
            var u = q.Dequeue();
            foreach (var v in graph[u])
                if (visited.Add(v)) q.Enqueue(v);
        }
        return visited;
    }
}