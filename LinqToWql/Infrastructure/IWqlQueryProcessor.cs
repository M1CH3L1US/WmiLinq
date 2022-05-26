using LinqToWql.Data;
using LinqToWql.Language;

namespace LinqToWql.Infrastructure;

public interface IWqlQueryProcessor {
  /// <summary>
  ///   Invokes a WQL query against the WMI resource.
  /// </summary>
  public IEnumerable<IResourceObject> ExecuteQuery(string query, QueryResultParseOptions parseOptions);
}