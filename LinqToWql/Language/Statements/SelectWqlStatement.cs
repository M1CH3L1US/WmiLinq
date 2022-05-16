using System.Linq.Expressions;
using LinqToWql.Language.Expressions;

namespace LinqToWql.Language.Statements; 

public class SelectWqlStatement : WqlStatement {
  public SelectWqlStatement(Expression innerStatement, List<PropertyWqlExpression> selectProperties) : base(innerStatement) {
    SelectProperties = selectProperties;
  }

  public List<PropertyWqlExpression> SelectProperties;
}