using System.Collections;
using LinqToWql.Language;

namespace LinqToWql.Infrastructure;

public interface IWqlQueryProcessor {
  /// <summary>
  ///   Invokes a WQL query against the WMI resource.
  ///   Returns an untyped IEnumerable using the type
  ///   specified in <see cref="parseOptions" />
  /// </summary>
  public IEnumerable ExecuteQuery(string query, QueryResultParseOptions parseOptions);

  /// <summary>
  ///   Invokes a WQL query against the WMI resource.
  ///   Returns a single item of type <see cref="T" />
  /// </summary>
  public T ExecuteQuery<T>(string query, QueryResultParseOptions parseOptions);
}