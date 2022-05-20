using System.Linq.Expressions;

namespace LinqToWql.Language;

public interface IWqlQueryRunner {
  public T Execute<T>(Expression query);
}