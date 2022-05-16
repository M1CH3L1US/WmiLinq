using System.Collections;

namespace LinqToWql.Infrastructure; 

public interface IWqlResourceContext {
  /// <summary>
  /// Invokes a WQL query against the WMI resource.
  /// Returns an untyped IEnumerable of IResultObject
  /// </summary>
  /// <param name="query"></param>
  /// <returns></returns>
  public IEnumerable InvokeQuery(string query);
 }