namespace ActiveStateMachine.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BuilderCodeFixProvider)), Shared]
public sealed class BuilderCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => ["ASM001", "ASM004", "ASM005"];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        foreach (var diagnostic in context.Diagnostics)
        {
            switch (diagnostic.Id)
            {
                case "ASM001": // No default state -> make the first State(...) default
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "Mark first state as default",
                            createChangedDocument: ct => MakeFirstStateDefaultAsync(context.Document, diagnostic.Location.SourceSpan, ct),
                            equivalenceKey: "ASM001_MarkDefault"),
                        diagnostic);
                    break;

                case "ASM004": // Missing target state -> add a State("Target")
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "Add missing target state",
                            createChangedDocument: ct => AddMissingTargetStateAsync(context, diagnostic, root, ct),
                            equivalenceKey: "ASM004_AddMissingState"),
                        diagnostic);
                    break;

                case "ASM005": // NEW: remove later duplicate On(...).…GoTo(...) chain
                    context.RegisterCodeFix(
                        CodeAction.Create(
                            title: "Remove duplicate trigger chain",
                            createChangedDocument: ct => RemoveDuplicateTriggerChainAsync(context.Document, diagnostic.Location.SourceSpan, ct),
                            equivalenceKey: "ASM005_RemoveDuplicate"),
                        diagnostic);
                    break;
            }
        }
    }

    private static async Task<Document> MakeFirstStateDefaultAsync(Document document, TextSpan span, CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null) return document;

        // Find first invocation ".State(...)" in the containing block
        var node = root.FindNode(span);
        var block = node.FirstAncestorOrSelf<BlockSyntax>();
        if (block is null) return document;

        var firstState = block.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .FirstOrDefault(inv => inv.Expression is MemberAccessExpressionSyntax m && m.Name.Identifier.ValueText == "State");

        if (firstState is null) return document;

        // If second argument is missing, add `, isDefault: true`
        var argList = firstState.ArgumentList;
        ArgumentSyntax defaultArg = SyntaxFactory.Argument(
            SyntaxFactory.NameColon("isDefault"),
            SyntaxFactory.Token(SyntaxKind.None),
            SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));

        var newArgList = argList.Arguments.Count == 1
            ? argList.WithArguments(argList.Arguments.Add(defaultArg))
            : argList;

        var newFirstState = firstState.WithArgumentList(newArgList);
        var newRoot = root.ReplaceNode(firstState, newFirstState);
        return document.WithSyntaxRoot(newRoot);
    }

    private static Task<Document> AddMissingTargetStateAsync(CodeFixContext context, Diagnostic diagnostic, SyntaxNode root, CancellationToken ct)
    {
        // We do not have the exact target literal location; parse it from the diagnostic message
        // message: "State '{source}' has transition to undefined target state '{target}'."
        var message = diagnostic.GetMessage();
        var target = ExtractBetween(message, "state '", "'.", last: true);
        if (string.IsNullOrEmpty(target))
            target = ExtractBetween(message, "target state '", "'");

        // Append a line: builder.State("Target");
        var node = root.FindNode(diagnostic.Location.SourceSpan);
        var block = node.FirstAncestorOrSelf<BlockSyntax>();
        if (block is null) return Task.FromResult(context.Document);

        var stateInvocation =
            SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        FindBuilderReceiver(block) ?? SyntaxFactory.IdentifierName("builder"),
                        SyntaxFactory.IdentifierName("State")))
                .WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList([
                            SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(target ?? "NewState")))
                        ]))));

        var newBlock = block.AddStatements(stateInvocation);
        var newRoot = root.ReplaceNode(block, newBlock);
        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
    }

    private static string? ExtractBetween(string s, string a, string b, bool last = false)
    {
        int start = last ? s.LastIndexOf(a, StringComparison.Ordinal) : s.IndexOf(a, StringComparison.Ordinal);
        if (start < 0) return null;
        start += a.Length;
        int end = s.IndexOf(b, start, StringComparison.Ordinal);
        if (end < 0) return null;
        return s.Substring(start, end - start);
    }

    private static ExpressionSyntax? FindBuilderReceiver(BlockSyntax block)
    {
        // Find a local named 'builder' as a best effort
        var local = block.DescendantNodes()
            .OfType<VariableDeclaratorSyntax>()
            .FirstOrDefault(v => v.Identifier.ValueText is "builder" or "b");

        if (local is not null)
            return SyntaxFactory.IdentifierName(local.Identifier.ValueText);

        return null;
    }

    private static async Task<Document> RemoveDuplicateTriggerChainAsync(Document document, TextSpan span, CancellationToken ct)
    {
        var root = await document.GetSyntaxRootAsync(ct).ConfigureAwait(false);
        if (root is null) return document;

        var node = root.FindNode(span);

        // We expect the diagnostic on the InvocationExpressionSyntax for .On(...)
        var onInvocation = node.FirstAncestorOrSelf<InvocationExpressionSyntax>();
        if (onInvocation is null) return document;

        // Climb to the top-most chain (ExpressionStatement) that starts with builder.State(...).On(...).*
        var exprStmt = onInvocation.FirstAncestorOrSelf<ExpressionStatementSyntax>();
        if (exprStmt is null) return document;

        // Remove the entire statement (practical and safe default)
        var newRoot = root.RemoveNode(exprStmt, SyntaxRemoveOptions.KeepExteriorTrivia);
        return document.WithSyntaxRoot(newRoot);
    }
}