using System.Collections;
using LinqToWql.Data;
using LinqToWql.Language;
using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Infrastructure;

public class WqlQueryProcessorAdapter : IWqlQueryProcessor {
  private readonly QueryProcessorBase _queryProcessor;

  public WqlQueryProcessorAdapter(QueryProcessorBase queryProcessor) {
    _queryProcessor = queryProcessor;
  }

  public IEnumerable ExecuteQuery(string query, QueryResultParseOptions parseOptions) {
    var result = _queryProcessor.ExecuteQuery(query);
    var resultEnumerable = ResultObjectEnumerableAdapter.FromResultObject(result);

    foreach (var resultObj in resultEnumerable) {
      var mapper = new ResultDataMapper(resultObj, parseOptions);
      yield return mapper.Map();
    }
  }

  public T ExecuteQuery<T>(string query, QueryResultParseOptions parseOptions) {
    parseOptions.QueryResultType = typeof(T);
    var queryResult = ExecuteQuery(query, parseOptions);
    return queryResult.Cast<T>().First();
  }
}