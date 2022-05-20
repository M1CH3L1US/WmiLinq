using System.Collections;
using LinqToWql.Data;
using LinqToWql.Infrastructure;
using LinqToWql.Language;
using LinqToWql.Test.Mocks.ResultObject;

namespace LinqToWql.Test.Mocks.Stubs;

public class StubWqlQueryProcessor : IWqlQueryProcessor {
  public IEnumerable ExecuteQuery(string query, QueryResultParseOptions parseOptions) {
    var mockData = new Dictionary<string, object> {
      {"Name", "Stub Collection"},
      {"Owner", "Michael"},
      {"SmsIds", "1--11--1"},
      {"CollectionId", "AE.BD.213:130"},
      {"Description", "Test Collection"}
    };
    var mockQueryResult = new StubResultObject(new List<Dictionary<string, object>> {mockData});
    var resultEnumerable = ResultObjectEnumerableAdapter.FromResultObject(mockQueryResult);

    return resultEnumerable
           .Select(obj => new ResultDataMapper(obj, parseOptions))
           .Select(mapper => mapper.Map());
  }

  public T ExecuteQuery<T>(string query, QueryResultParseOptions parseOptions) {
    parseOptions.QueryResultType = typeof(T);
    var queryResult = ExecuteQuery(query, parseOptions);
    return queryResult.Cast<T>().Single();
  }
}