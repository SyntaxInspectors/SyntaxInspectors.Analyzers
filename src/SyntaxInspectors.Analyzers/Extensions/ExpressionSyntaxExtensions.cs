using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SyntaxInspectors.Analyzers.Extensions;

public static class ExpressionSyntaxExtensions
{
    public static ExpressionSyntax GetFirstNonCastExpression(this ExpressionSyntax expression)
    {
        while (true)
        {
            if (expression is not CastExpressionSyntax castExpression)
            {
                return expression;
            }

            expression = castExpression.Expression;
        }
    }
}
