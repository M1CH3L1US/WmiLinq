using System.Linq.Expressions;
using LinqToWql.Infrastructure;

namespace LinqToWql.Language;

public interface IWqlQueryRunner {
  public T Execute<T>(Expression query, WqlResourceContext context);
}