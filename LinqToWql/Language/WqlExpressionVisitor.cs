using System.Linq.Expressions;
using LinqToWql.Language.Expressions;

namespace LinqToWql.Language;

public class WqlExpressionVisitor : QueryableExpressionVisitor {
  protected override Expression TranslateOrWhere(Expression source, LambdaExpression lambdaExpression) {
    return Builder(source)
           .AddWhereClauseFromLambda(lambdaExpression, ExpressionChainType.Or)
           .Build();
  }

  protected override Expression TranslateWhere(Expression source, LambdaExpression lambdaExpression) {
    return Builder(source)
           .AddWhereClauseFromLambda(lambdaExpression)
           .Build();
  }

  protected override Expression TranslateSelect(Expression source, LambdaExpression lambdaExpression) {
    return Builder(source)
           .AddSelectClauseFromLambda(lambdaExpression)
           .Build();
  }

  protected override Expression TranslateSingle(Expression source, LambdaExpression? lambdaExpression) {
    return Builder(source)
           .TryAddWhereClauseFromLambda(lambdaExpression)
           .AddQueryResultProcessor(result => result.Single())
           .Build();
  }

  protected override Expression TranslateSingleOrDefault(Expression source, LambdaExpression? lambdaExpression) {
    return Builder(source)
           .TryAddWhereClauseFromLambda(lambdaExpression)
           .AddQueryResultProcessor(result => result.SingleOrDefault())
           .Build();
  }

  protected override Expression TranslateCount(Expression source) {
    return Builder(source)
           .AddQueryResultProcessor(result => result.Count())
           .Build();
  }

  protected override Expression TranslateWithin(Expression source, ConstantExpression timeout) {
    throw new NotImplementedException();
  }

  protected override Expression TranslateHaving(Expression source, LambdaExpression predicate) {
    throw new NotImplementedException();
  }

  private WqlStatementBuilder Builder(Expression source) {
    return new WqlStatementBuilder(source);
  }

  //protected override Expression TranslateAssociatorsOf(Expression source, ConstantExpression objectPath) {
  //  throw new NotImplementedException();
  //}
}