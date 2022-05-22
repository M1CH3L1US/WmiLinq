using System.Linq.Expressions;

namespace LinqToWql.Language.Statements;

public class WqlStatement : Expression {
  private readonly List<Action<WqlQueryBuilder>> _builderActions;
  public Expression InnerStatement;

  public WqlStatement(Expression innerStatement, List<Action<WqlQueryBuilder>> builderActions) {
    InnerStatement = innerStatement;
    _builderActions = builderActions;
  }

  public void AppendSelfToQuery(WqlQueryBuilder builder) {
    foreach (var builderAction in _builderActions) {
      builderAction(builder);
    }
  }
}