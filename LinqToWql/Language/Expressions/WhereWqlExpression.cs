using System.Linq.Expressions;
using System.Text;

namespace LinqToWql.Language.Expressions; 

public class WhereWqlExpression : WqlStatement {
  public WqlExpression InnerExpression;

  public WhereWqlExpression(Expression? innerStatement, WqlExpression innerExpression) : base(innerStatement) {
    InnerExpression = innerExpression;
  }
}