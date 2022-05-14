using System.Linq.Expressions;
using LinqToWql.Language.Expressions;

namespace LinqToWql.Language;

public class WqlExpressionVisitor : QueryableExpressionVisitor {
  private readonly SqlExpressionFactory _factory = new ();

  protected override Expression TranslateWhere(Expression expression, LambdaExpression lambda) {
    return _factory.MakeWhereExpression(expression, lambda);
  }
  
  protected override Expression TranslateSelect(Expression expression, LambdaExpression lambda) {
    return _factory.MakeSelectExpression(expression, lambda);
  }
  
  protected override Expression TranslateOr(Expression source) {
    throw new NotImplementedException();
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