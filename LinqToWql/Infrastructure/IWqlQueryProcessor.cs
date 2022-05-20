using LinqToWql.Language;

namespace LinqToWql.Infrastructure;

public interface IWqlQueryProcessor {
  /// <summary>
  ///   Invokes a WQL query against the WMI resource.
  /// </summary>
  public T ExecuteQuery<T>(string query, QueryResultParseOptions parseOptions);
}