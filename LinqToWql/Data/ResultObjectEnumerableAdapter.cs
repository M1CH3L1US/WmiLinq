using System.Collections;
using LinqToWql.Infrastructure;
using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Data;

/// <summary>
///   Wrapper for IResultObject to easily iterate over it's
///   result items.
/// </summary>
public class ResultObjectEnumerableAdapter : IEnumerable<IResourceObject> {
  private readonly IEnumerator _enumerator;
  private readonly WqlResourceContext _context;

  protected ResultObjectEnumerableAdapter(IEnumerator enumerator, WqlResourceContext context)
  {
    _enumerator = enumerator;
    _context = context;
  }

  public IEnumerator<IResourceObject> GetEnumerator() {
    return EnumerateAndCast();
  }

  IEnumerator IEnumerable.GetEnumerator() {
    return EnumerateAndCast();
  }

  public static IEnumerable<IResourceObject> FromResultObject(IResultObject resultObject, WqlResourceContext context) {
    var enumerator = resultObject.GetEnumerator();
    return new ResultObjectEnumerableAdapter(enumerator, context);
  }

  private IEnumerator<IResourceObject> EnumerateAndCast() {
    while (_enumerator.MoveNext()) {
      yield return WrapResultObject((IResultObject)_enumerator.Current) ;
    }
  }

  private IResourceObject WrapResultObject(IResultObject obj) {
    return new ResultObjectAdapter(_context, obj);
  }
}