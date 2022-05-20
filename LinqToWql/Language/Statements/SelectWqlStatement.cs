using System.Linq.Expressions;
using LinqToWql.Language.Expressions;

namespace LinqToWql.Language.Statements;

public class SelectWqlStatement : WqlStatement {
  public List<PropertyWqlExpression> SelectProperties;

  /// <summary>
  ///   Whether the select statement reduces the result to a single
  ///   property value.
  /// </summary>
  public bool SelectToSingleProperty;

  public SelectWqlStatement(Expression innerStatement, List<PropertyWqlExpression> selectProperties) :
    base(innerStatement) {
    SelectProperties = selectProperties;

    if (selectProperties.Count == 1) {
      SelectToSingleProperty = true;
    }
  }
}