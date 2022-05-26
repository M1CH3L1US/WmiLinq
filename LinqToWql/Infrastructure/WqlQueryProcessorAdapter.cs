using LinqToWql.Data;
using LinqToWql.Language;
using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Infrastructure;

public class WqlQueryProcessorAdapter : IWqlQueryProcessor {
  private readonly QueryProcessorBase _queryProcessor;

  public WqlQueryProcessorAdapter(QueryProcessorBase queryProcessor) {
    _queryProcessor = queryProcessor;
  }


  public IEnumerable<IResourceObject> ExecuteQuery(string query, QueryResultParseOptions parseOptions) {
    var queryResult = _queryProcessor.ExecuteQuery(query);
    return ResultObjectEnumerableAdapter.FromResultObject(queryResult, parseOptions.Context);
  }
}