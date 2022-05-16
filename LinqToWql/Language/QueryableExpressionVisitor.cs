using System.Linq.Expressions;

namespace LinqToWql.Language;

public abstract class QueryableExpressionVisitor : ExpressionVisitor {
  protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression) {
    var method = methodCallExpression.Method;
    
    if (method.DeclaringType != typeof(Queryable) && method.DeclaringType != typeof(QueryableExtensions)) {
      throw new NotSupportedException($"Method {method.Name} is not supported");
    }

    LambdaExpression GetLambdaExpressionFromArgument(int argumentIndex) {
      return UnwrapLambdaFromQuote(methodCallExpression.Arguments[argumentIndex]);
    }
    
    LambdaExpression UnwrapLambdaFromQuote(Expression expression) {
      if (expression is UnaryExpression {NodeType: ExpressionType.Quote} unary) {
        return (LambdaExpression) unary.Operand;
      }

      return (LambdaExpression) expression;
    }

    ConstantExpression GetConstExpressionFromArgument(int argumentIndex) {
      return (ConstantExpression) methodCallExpression.Arguments[argumentIndex];
    }
    
    var sourceExpr = Visit(methodCallExpression.Arguments[0]);

    return method.Name switch {
      // Custom methods
      nameof(QueryableExtensions.Having) =>
        TranslateHaving(sourceExpr, GetLambdaExpressionFromArgument(1)),
      nameof(QueryableExtensions.Within) =>
        TranslateWithin(sourceExpr, GetConstExpressionFromArgument(1)),
      nameof(QueryableExtensions.OrWhere) =>
        TranslateOrWhere(sourceExpr, GetLambdaExpressionFromArgument(1)),
      //
      nameof(Queryable.Where) =>
        TranslateWhere(sourceExpr, GetLambdaExpressionFromArgument(1)),
      nameof(Queryable.Select) =>
        TranslateSelect(sourceExpr, GetLambdaExpressionFromArgument(1)),
      _ => null!,
    };
  }

  protected override Expression VisitExtension(Expression expression) {
    return expression switch {
      _ => base.VisitExtension(expression)
    };
  }
  protected abstract Expression TranslateOrWhere(Expression soruce, LambdaExpression lambdaExpression);
  protected abstract Expression TranslateWhere(Expression source, LambdaExpression lambdaExpression);
  protected abstract Expression TranslateSelect(Expression expression, LambdaExpression lambdaExpression);
  
  # region WQL Methods

  protected abstract Expression TranslateWithin(Expression source, ConstantExpression timeout);
  protected abstract Expression TranslateHaving(Expression source, LambdaExpression predicate);
  // protected abstract Expression TranslateAssociatorsOf(Expression source, ConstantExpression objectPath);

  #endregion
}