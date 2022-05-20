using System.Collections;
using System.Linq.Expressions;

namespace LinqToWql.Language;

public interface IWqlQueryRunner {
  /// <summary>
  ///   Returns an enumerator of type <see cref="queryResultType" />
  /// </summary>
  /// <param name="query"></param>
  /// <param name="queryResultType"></param>
  /// <returns></returns>
  public IEnumerable Execute(Expression query, Type queryResultType);

  /// <summary>
  ///   Returns a single item of Type T
  /// </summary>
  /// <param name="query"></param>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  public T Execute<T>(Expression query);
}