using System.Linq.Expressions;

namespace LinqToWql.Language.Expressions;

public class WqlStatement : Expression {
  public Expression InnerStatement;

  public WqlStatement(Expression innerStatement) {
    InnerStatement = innerStatement;
  }
}