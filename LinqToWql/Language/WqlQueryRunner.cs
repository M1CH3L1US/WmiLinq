using System.Linq.Expressions;
using LinqToWql.Data;
using LinqToWql.Infrastructure;
using LinqToWql.Language.Statements;

namespace LinqToWql.Language;

public class WqlQueryRunner : IWqlQueryRunner {
  private readonly IWqlQueryProcessor _queryProcessor;

  public WqlQueryRunner(IWqlQueryProcessor queryProcessor) {
    _queryProcessor = queryProcessor;
  }

  public T Execute<T>(Expression query, WqlResourceContext context) {
    var queryString = MakeQueryString(query, context, out var parseOptions);
    var queryResult = _queryProcessor.ExecuteQuery(queryString, parseOptions);

    // The ToList call ensures that the enumerations of the items have taken place
    // such that we no longer rely on the enumerator of IResultObject that might
    // still have a connection to the wql query server.
    var resultObjectsAsList = queryResult.ToList();

    var mapper = new ResultDataMapper<T>(resultObjectsAsList, parseOptions);
    var mappedResultObject = mapper.ApplyTypeMapping();

    if (typeof(T).IsEnumerable(out var enumerableType)) {
      //  We need this additional conversion step through
      //  Enumerable.Cast[TResult] because we cannot cast
      //  List[object] or IEnumerable[object] to T,
      //  which is IEnumerable[TResult]
      return (T) ((IEnumerable<object>) mappedResultObject).RuntimeCast(enumerableType);
    }

    return (T) mappedResultObject;
  }

  private string MakeQueryString(Expression query, WqlResourceContext context,
    out QueryResultParseOptions parseOptions) {
    var visitor = new WqlExpressionVisitor();
    var wqlExpressionTree = visitor.Visit(query) as WqlStatement;

    if (wqlExpressionTree is null) {
      ThrowNotSupported(query);
    }

    var wqlBuilder = new WqlQueryBuilder(wqlExpressionTree, context);

    var queryStatement = wqlBuilder.Build(out var options);
    parseOptions = options;
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