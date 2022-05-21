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

    // If the result type is not an IEnumerable, we only return
    // The first item because this means the result was not requested
    // by IQueryable but instead through .Single or .First. 
    if (!IsIEnumerable<T>(out _)) {
      return (T) MapToResultType(resultEnumerable.First(), parseOptions);
    }

    // The ToList call ensures that the enumerations of the items have taken place
    // such that we no longer rely on the enumerator of IResultObject that might
    // still have a connection to the wql query server.  
    var listResult = resultEnumerable.ToList();

    return CastToResultEnumerable<T>(listResult, parseOptions);
  }

  private T CastToResultEnumerable<T>(
    IEnumerable<IResultObject> resultObjects,
    QueryResultParseOptions parseOptions
  ) {
    var resultType = parseOptions.QueryResultType;
    var genericCastMethod = typeof(Enumerable)
                            .GetMethod(nameof(Enumerable.Cast))!
                            .MakeGenericMethod(resultType);

    var mappedResult = resultObjects.Select(obj => MapToResultType(obj, parseOptions));
    return (T) genericCastMethod.Invoke(null, new object[] {mappedResult})!;
  }

  private object MapToResultType(IResultObject resultObject, QueryResultParseOptions parseOptions) {
    var mapper = new ResultDataMapper(resultObject, parseOptions);
    return mapper.ApplyTypeMapping();
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