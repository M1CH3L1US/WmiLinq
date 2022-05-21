using System.Linq.Expressions;
using LinqToWql.Language.Expressions;
using LinqToWql.Language.Statements;

namespace LinqToWql.Language;

public class WqlExpressionVisitor : QueryableExpressionVisitor {
  private readonly WqlExpressionFactory _factory = new();

  protected override Expression TranslateOrWhere(Expression source, LambdaExpression lambdaExpression) {
    return _factory.MakeWhereExpression(source, lambdaExpression, ExpressionChainType.Or);
  }

  protected override Expression TranslateWhere(Expression source, LambdaExpression lambda) {
    return _factory.MakeWhereExpression(source, lambda, ExpressionChainType.And);
  }

  protected override Expression TranslateSelect(Expression source, LambdaExpression lambda) {
    return _factory.MakeSelectExpression(source, lambda);
  }

  protected override Expression TranslateSingle(Expression source, LambdaExpression? lambdaExpression) {
    return new EmptyWqlStatement(source);
  }

  protected override Expression TranslateCount(Expression source) {
    return new EmptyWqlStatement(source);
  }

  protected override Expression TranslateWithin(Expression source, ConstantExpression timeout) {
    throw new NotImplementedException();
  }

  protected override Expression TranslateHaving(Expression source, LambdaExpression predicate) {
    throw new NotImplementedException();
  }

  //protected override Expression TranslateAssociatorsOf(Expression source, ConstantExpression objectPath) {
  //  throw new NotImplementedException();
  //}
}