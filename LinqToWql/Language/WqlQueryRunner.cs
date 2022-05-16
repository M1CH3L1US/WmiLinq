using System.Linq.Expressions;
using LinqToWql.Infrastructure;
using LinqToWql.Language.Expressions;

namespace LinqToWql.Language; 

public class WqlQueryRunner : IWqlQueryRunner {
  private readonly IWqlResourceContext _context;

  public WqlQueryRunner(IWqlResourceContext context) {
    _context = context;
  }

  public T Execute<T>(Expression query) {
    var visitor = new WqlExpressionVisitor();
    var wqlExpressionTree = (WqlStatement) visitor.Visit(query);
    var wqlBuilder = new WqlQueryBuilder(wqlExpressionTree);

    var queryStatement = wqlBuilder.Build();
    
    return (T)_context.InvokeQuery(queryStatement);
  }
}