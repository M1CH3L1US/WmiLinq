using System.Linq.Expressions;
using System.Reflection;
using LinqToWql.Language;

namespace LinqToWql.Infrastructure;

/// <summary>
///   A wrapper class for <see cref="IWqlQueryRunner" />, implementing
///   IQueryProvider to support LINQ query execution.
/// </summary>
public class WqlQueryProvider : IQueryProvider {
  private static readonly MethodInfo _createQueryMethod = GetGenericMethod(nameof(CreateQuery));
  private static readonly MethodInfo _executeMethod = GetGenericMethod(nameof(Execute));
  private readonly WqlResourceContext _context;
  private readonly IWqlQueryRunner _runner;

  public WqlQueryProvider(WqlResourceContext context, IWqlQueryRunner runner) {
    _context = context;
    _runner = runner;
  }

  public IQueryable CreateQuery(Expression expression) {
    var genericMakeQueryMethod = _createQueryMethod.MakeGenericMethod(expression.Type);
    return (IQueryable) genericMakeQueryMethod.Invoke(this, new object[] {expression})!;
  }

  public IQueryable<T> CreateQuery<T>(Expression expression) {
    return new WqlResource<T>(this, expression);
  }

  public object? Execute(Expression expression) {
    var genericExecuteMethod = _executeMethod.MakeGenericMethod(expression.Type);
    return genericExecuteMethod.Invoke(this, new object[] {expression})!;
  }

  public T Execute<T>(Expression expression) {
    return _runner.Execute<T>(expression, _context);
  }

  private static MethodInfo GetGenericMethod(string name) {
    return typeof(WqlQueryProvider)
           .GetMethods()
           .Single(m => m.Name == name && m.IsGenericMethod);
  }
}