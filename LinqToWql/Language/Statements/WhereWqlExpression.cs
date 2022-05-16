using System.Linq.Expressions;
using LinqToWql.Language.Expressions;

namespace LinqToWql.Language.Statements; 

public class WhereWqlExpression : WqlStatement {
  public WqlExpression InnerExpression;
  public ExpressionChainType ChainType;
  
  public WhereWqlExpression(
    Expression innerStatement,
    WqlExpression innerExpression,
    ExpressionChainType chainType = ExpressionChainType.And
    ) : base(innerStatement) {
    InnerExpression = innerExpression;
    ChainType = chainType;
  }
}