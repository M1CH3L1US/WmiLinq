using System.Linq.Expressions;
using LinqToWql.Language.Expressions;

namespace LinqToWql.Language; 

public class WqlQueryCompiler : IQueryCompiler {
  public TResult Execute<TResult>(Expression query) {
    var visitor = new WqlExpressionVisitor();
    var wqlExpressionTree = (WqlStatement) visitor.Visit(query);
    var wqlbuilder = new WqlQueryBuilder(wqlExpressionTree);

    return (TResult) default;
  }
}