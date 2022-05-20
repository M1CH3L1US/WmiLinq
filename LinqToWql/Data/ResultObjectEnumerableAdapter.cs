using System.Collections;
using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Data;

/// <summary>
///   Wrapper for IResultObject to easily iterate over it's
///   result items.
/// </summary>
public class ResultObjectEnumerableAdapter : IEnumerable<IResultObject> {
  private readonly IEnumerator _enumerator;

  protected ResultObjectEnumerableAdapter(IEnumerator enumerator) {
    _enumerator = enumerator;
  }

  public IEnumerator<IResultObject> GetEnumerator() {
    return EnumerateAndCast();
  }

  IEnumerator IEnumerable.GetEnumerator() {
    return EnumerateAndCast();
  }

  public static IEnumerable<IResultObject> FromResultObject(IResultObject resultObject) {
    var enumerator = resultObject.GetEnumerator();
    return new ResultObjectEnumerableAdapter(enumerator);
  }

  private IEnumerator<IResultObject> EnumerateAndCast() {
    while (_enumerator.MoveNext()) {
      yield return (IResultObject) _enumerator.Current;
    }
  }
}