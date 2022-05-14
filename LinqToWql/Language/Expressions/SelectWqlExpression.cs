using System.Linq.Expressions;

namespace LinqToWql.Language.Expressions; 

public class SelectWqlExpression : WqlStatement {
  public SelectWqlExpression(Expression innerStatement, List<PropertyWqlExpression> selectProperties) : base(innerStatement) {
    SelectProperties = selectProperties;
  }

  public List<PropertyWqlExpression> SelectProperties;
}