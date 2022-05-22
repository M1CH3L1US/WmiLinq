using LinqToWql.Data;
using LinqToWql.Language;
using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Infrastructure;

public class WqlQueryProcessorAdapter : IWqlQueryProcessor {
  private readonly QueryProcessorBase _queryProcessor;

  public WqlQueryProcessorAdapter(QueryProcessorBase queryProcessor) {
    _queryProcessor = queryProcessor;
  }

  public T ExecuteQuery<T>(string query, QueryResultParseOptions parseOptions) {
    parseOptions.QueryResultType = GetResultType<T>();
    var queryResult = _queryProcessor.ExecuteQuery(query);
    var resultEnumerable = ResultObjectEnumerableAdapter.FromResultObject(queryResult);

    // The ToList call ensures that the enumerations of the items have taken place
    // such that we no longer rely on the enumerator of IResultObject that might
    // still have a connection to the wql query server.
    var resultObjectsAsList = resultEnumerable.ToList();

    var mapper = new ResultDataMapper(resultObjectsAsList, parseOptions);
    var mappedResultObject = mapper.ApplyTypeMapping();

    if (IsIEnumerable<T>(out _)) {
      return CastToResultEnumerable<T>((IEnumerable<object>) mappedResultObject, parseOptions);
    }

    return (T) mappedResultObject;
  }

  /// <summary>
  ///   We need this additional conversion step through
  ///   Enumerable.Cast[TResult] because we cannot cast
  ///   List[object] or IEnumerable[object] to T,
  ///   which is IEnumerable[TResult]
  /// </summary>
  /// <param name="objects"></param>
  /// <param name="parseOptions"></param>
  /// <typeparam name="T"></typeparam>
  private T CastToResultEnumerable<T>(
    IEnumerable<object> objects,
    QueryResultParseOptions parseOptions
  ) {
    var resultType = parseOptions.QueryResultType;
    var genericCastMethod = typeof(Enumerable)
                            .GetMethod(nameof(Enumerable.Cast))!
                            .MakeGenericMethod(resultType);

    return (T) genericCastMethod.Invoke(null, new object[] {objects})!;
  }

  private Type GetResultType<T>() {
    if (IsIEnumerable<T>(out var enumerableType)) {
      return enumerableType!;
    }

    return typeof(T);
  }

  /// <summary>
  ///   Checks whether T is an IEnumerable, meaning that
  ///   the result type of the query is expected to be an
  ///   enumerable type.
  /// </summary>
  /// <param name="enumerableType"></param>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  private bool IsIEnumerable<T>(out Type? enumerableType) {
    enumerableType = null;
    var type = typeof(T);

    if (!type.IsGenericType) {
      return false;
    }

    if (type.GetGenericTypeDefinition() != typeof(IEnumerable<>)) {
      return false;
    }

    enumerableType = type.GetGenericArguments().First();
    return true;
  }
}