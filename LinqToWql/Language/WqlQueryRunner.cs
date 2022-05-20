using System.Collections;
using System.Linq.Expressions;
using LinqToWql.Infrastructure;
using LinqToWql.Language.Expressions;

namespace LinqToWql.Language;

public class WqlQueryRunner : IWqlQueryRunner {
  private readonly IWqlQueryProcessor _queryProcessor;

  public WqlQueryRunner(WqlResourceContext context) {
    _queryProcessor = context.QueryProcessor;
  }

  public IEnumerable Execute(Expression query, Type queryResultType) {
    var queryString = MakeQueryString(query, out var parseOptions);
    return _queryProcessor.ExecuteQuery(queryString, parseOptions);
  }

  public T Execute<T>(Expression query) {
    var queryString = MakeQueryString(query, out var parseOptions);
    return _queryProcessor.ExecuteQuery<T>(queryString, parseOptions);
  }

  private string MakeQueryString(Expression query, out QueryResultParseOptions parseOptions) {
    var visitor = new WqlExpressionVisitor();
    var wqlExpressionTree = (WqlStatement) visitor.Visit(query)!;
    var wqlBuilder = new WqlQueryBuilder(wqlExpressionTree);

    var queryStatement = wqlBuilder.Build(out var options);
    parseOptions = options;
    return queryStatement;
  }
}