using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SyntaxInspectors.Analyzers.Extensions;

namespace SyntaxInspectors.Analyzers.Diagnosers.UseIsForNullComparison;

public static class LinqIdentifierResolver
{
    public static SyntaxNode? Resolve(QueryExpressionSyntax startingPoint, string identifierName)
    {
        var walker = new Walker(identifierName);
        walker.Visit(startingPoint);
        return walker.IdentifierSource;
    }

    private sealed class Walker : CSharpSyntaxWalker
    {
        private readonly string _identifierNameToFind;

        public Walker(string identifierNameToFind)
        {
            _identifierNameToFind = identifierNameToFind;
        }

        public SyntaxNode? IdentifierSource { get; private set; }

        public override void VisitFromClause(FromClauseSyntax node)
        {
            if (node.Identifier.ValueText.EqualsOrdinal(_identifierNameToFind))
            {
                IdentifierSource = node.Expression;
                return;
            }

            base.VisitFromClause(node);
        }

        public override void VisitJoinClause(JoinClauseSyntax node)
        {
            if (node.Identifier.ValueText.EqualsOrdinal(_identifierNameToFind))
            {
                IdentifierSource = node.InExpression ;
                return;
            }

            base.VisitJoinClause(node);
        }
    }
}
