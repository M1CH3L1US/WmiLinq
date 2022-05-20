using LinqToWql.Data;
using LinqToWql.Infrastructure;
using LinqToWql.Language;
using LinqToWql.Test.Mocks.ResultObject;
using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Test.Mocks.Stubs;

public class StubWqlQueryProcessor : IWqlQueryProcessor {
  public T ExecuteQuery<T>(string query, QueryResultParseOptions parseOptions) {
    parseOptions.QueryResultType = GetResultType<T>();
    var mockData = new Dictionary<string, object> {
      {"Name", "Stub Collection"},
      {"Owner", "Michael"},
      {"SmsIds", "1--11--1"},
      {"CollectionId", "AE.BD.213:130"},
      {"Description", "Test Collection"}
    };
    var mockQueryResult = new StubResultObject(new List<Dictionary<string, object>> {mockData});
    var resultEnumerable = ResultObjectEnumerableAdapter.FromResultObject(mockQueryResult);

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