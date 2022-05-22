using System.Linq.Expressions;
using LinqToWql.Infrastructure;
using LinqToWql.Language.Statements;

namespace LinqToWql.Language;

public class WqlQueryRunner : IWqlQueryRunner {
  private readonly WqlResourceContext _context;

  public WqlQueryRunner(WqlResourceContext context) {
    _context = context;
  }

  public T Execute<T>(Expression query) {
    var queryString = MakeQueryString(query, out var parseOptions);
    return _context.QueryProcessor.ExecuteQuery<T>(queryString, parseOptions);
  }

  private string MakeQueryString(Expression query, out QueryResultParseOptions parseOptions) {
    var visitor = new WqlExpressionVisitor();
    var wqlExpressionTree = visitor.Visit(query) as WqlStatement;

    if (wqlExpressionTree is null) {
      ThrowNotSupported(query);
    }

    var wqlBuilder = new WqlQueryBuilder(wqlExpressionTree);

    var queryStatement = wqlBuilder.Build(out var options);
    parseOptions = options;
    parseOptions.Context = _context;
    return queryStatement;
  }

  private void ThrowNotSupported(Expression expression) {
    if (expression is not MethodCallExpression methodCallExpression) {
      throw new NotSupportedException($"Unknown operation on WqlResource is not supported: {expression}");
    }

    var methodName = methodCallExpression.Method.Name;
    throw new NotSupportedException(
      $"Method '{methodName}' is not supported or implemented. Usage context: {expression}");
  }
}