using System.Linq.Expressions;
using System.Reflection.Metadata;
using Microsoft.EntityFrameworkCore.Query;

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
    var genericMethod = method.IsGenericMethod ? method.GetGenericMethodDefinition() : null;

    return genericMethod switch {
      // Custom methods
      _ when genericMethod == QueryableExtensions.HavingMethodInfo =>
        TranslateHaving(sourceExpr, GetLambdaExpressionFromArgument(1)),
      _ when genericMethod == QueryableExtensions.WithinMethodInfo =>
        TranslateWithin(sourceExpr, GetConstExpressionFromArgument(1)),
     // _ when genericMethod == QueryableExtensions.AssociatorsOfMethodInfo =>
     //   TranslateAssociatorsOf(sourceExpr, GetConstExpressionFromArgument(1)),
      _ when genericMethod == QueryableExtensions.OrMethodInfo =>
        TranslateOr(sourceExpr),
      //
      _ when genericMethod == QueryableMethods.Where =>
        TranslateWhere(sourceExpr, GetLambdaExpressionFromArgument(1)),
      _ when genericMethod == QueryableMethods.Select =>
        TranslateSelect(sourceExpr, GetLambdaExpressionFromArgument(1)),
      _ => null!,
    };
  }

  protected override Expression VisitExtension(Expression expression) {
    return expression switch {
      _ => base.VisitExtension(expression)
    };
  }
    
  protected abstract Expression TranslateWhere(Expression source, LambdaExpression lambdaExpression);
  protected abstract Expression TranslateSelect(Expression expression, LambdaExpression lambdaExpression);
  
  # region WQL Methods

  protected abstract Expression TranslateWithin(Expression source, ConstantExpression timeout);
  protected abstract Expression TranslateHaving(Expression source, LambdaExpression predicate);
  protected abstract Expression TranslateOr(Expression source);
  // protected abstract Expression TranslateIsA(Expression source, ConstantExpression comparisonType);
  // protected abstract Expression TranslateAssociatorsOf(Expression source, ConstantExpression objectPath);

  #endregion
}