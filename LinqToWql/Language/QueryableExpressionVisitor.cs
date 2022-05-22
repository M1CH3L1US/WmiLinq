using System.Linq.Expressions;

namespace LinqToWql.Language;

public abstract class QueryableExpressionVisitor : ExpressionVisitor {
  protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression) {
    var method = methodCallExpression.Method;

    if (method.DeclaringType != typeof(Queryable) && method.DeclaringType != typeof(QueryableExtensions)) {
      throw new NotSupportedException($"Method {method.Name} is not supported");
    }

    LambdaExpression? TryGetLambdaExpressionFromArgument(int argumentIndex) {
      try {
        var argument = methodCallExpression.Arguments[argumentIndex];
        return UnwrapLambdaFromQuote(argument);
      }
      catch {
        // ignored
      }

      return null;
    }

    LambdaExpression GetLambdaExpressionFromArgument(int argumentIndex) {
      return UnwrapLambdaFromQuote(methodCallExpression.Arguments[argumentIndex]);
    }

    LambdaExpression UnwrapLambdaFromQuote(Expression? expression) {
      if (expression is UnaryExpression {NodeType: ExpressionType.Quote} unary) {
        return (LambdaExpression) unary.Operand;
      }

      return (LambdaExpression) expression;
    }

    ConstantExpression GetConstExpressionFromArgument(int argumentIndex) {
      return (ConstantExpression) methodCallExpression.Arguments[argumentIndex];
    }

    var source = Visit(methodCallExpression.Arguments[0])!;

    return method.Name switch {
      // Custom methods
      nameof(QueryableExtensions.Having) =>
        TranslateHaving(source, GetLambdaExpressionFromArgument(1)),
      nameof(QueryableExtensions.Within) =>
        TranslateWithin(source, GetConstExpressionFromArgument(1)),
      nameof(QueryableExtensions.OrWhere) =>
        TranslateOrWhere(source, GetLambdaExpressionFromArgument(1)),
      //
      nameof(Queryable.Where) =>
        TranslateWhere(source, GetLambdaExpressionFromArgument(1)),
      nameof(Queryable.Select) =>
        TranslateSelect(source, GetLambdaExpressionFromArgument(1)),
      nameof(Queryable.Single) =>
        TranslateSingle(source, TryGetLambdaExpressionFromArgument(1)),
      nameof(Queryable.Count) =>
        TranslateCount(source),
      nameof(Queryable.SingleOrDefault) =>
        TranslateSingleOrDefault(source, TryGetLambdaExpressionFromArgument(1)),
      nameof(Queryable.All) =>
        TranslateAll(source, GetLambdaExpressionFromArgument(1)),
      nameof(Queryable.Any) =>
        TranslateAny(source, TryGetLambdaExpressionFromArgument(1)),
      nameof(Queryable.First) =>
        TranslateFirst(source, TryGetLambdaExpressionFromArgument(1)),
      nameof(Queryable.Skip) =>
        TranslateSkip(source, GetConstExpressionFromArgument(1)),
      nameof(Queryable.Take) =>
        TranslateTake(source, GetConstExpressionFromArgument(1)),
      nameof(Enumerable.AsEnumerable) =>
        TranslateAsEnumerable(source),
      _ => null!
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
  protected abstract Expression TranslateSingle(Expression source, LambdaExpression? lambdaExpression);
  protected abstract Expression TranslateCount(Expression source);
  protected abstract Expression TranslateSingleOrDefault(Expression source, LambdaExpression? lambdaExpression);
  protected abstract Expression TranslateAll(Expression source, LambdaExpression lambda);
  protected abstract Expression TranslateAny(Expression source, LambdaExpression? lambda);
  protected abstract Expression TranslateFirst(Expression source, LambdaExpression? lambda);
  protected abstract Expression TranslateSkip(Expression source, ConstantExpression count);
  protected abstract Expression TranslateTake(Expression source, ConstantExpression count);
  protected abstract Expression TranslateAsEnumerable(Expression source);

  # region WQL Methods

  protected abstract Expression TranslateWithin(Expression source, ConstantExpression timeout);

  protected abstract Expression TranslateHaving(Expression source, LambdaExpression predicate);
  // protected abstract Expression TranslateAssociatorsOf(Expression source, ConstantExpression objectPath);

  #endregion
}