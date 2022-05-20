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

    if (!IsIEnumerator<T>(out _)) {
      return (T) MapToResultType(resultEnumerable.First(), parseOptions);
    }

    return CastToResultEnumerable<T>(resultEnumerable, parseOptions);
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
    if (IsIEnumerator<T>(out var enumeratorType)) {
      return enumeratorType!;
    }

    return typeof(T);
  }

  private bool IsIEnumerator<T>(out Type? enumeratorType) {
    enumeratorType = null;
    var type = typeof(T);

    if (!type.IsGenericType) {
      return false;
    }

    if (type.GetGenericTypeDefinition() != typeof(IEnumerator<>)) {
      return false;
    }

    enumeratorType = type.GetGenericArguments().First();
    return true;
  }
}