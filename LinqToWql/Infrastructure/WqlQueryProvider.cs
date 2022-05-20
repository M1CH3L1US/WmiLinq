using System.Linq.Expressions;
using System.Reflection;
using LinqToWql.Language;

namespace LinqToWql.Infrastructure;

public class WqlQueryProvider : IQueryProvider {
  private static readonly MethodInfo _createQueryMethod = GetGenericMethod(nameof(CreateQuery));
  private static readonly MethodInfo _executeMethod = GetGenericMethod(nameof(Execute));
  private readonly IWqlQueryRunner _runner;

  public WqlQueryProvider(IWqlQueryRunner wqlQueryRunner) {
    _runner = wqlQueryRunner;
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
    // T is either TSource of IQueryable or IEnumerator<T> if
    // invoked through IQueryableProvider.Execute
    if (IsQueryResultEnumerator<T>()) {
      var queryResultType = typeof(T).GetGenericArguments().First()!;
      return (T) _runner.Execute(expression, queryResultType);
    }

    var result = _runner.Execute<T>(expression);
    return result;
  }

  private bool IsQueryResultEnumerator<T>() {
    var resultType = typeof(T);
    if (!resultType.IsGenericType) {
      return false;
    }

    return resultType.GetGenericTypeDefinition() == typeof(IEnumerator<>);
  }

  private static MethodInfo GetGenericMethod(string name) {
    return typeof(WqlQueryProvider)
           .GetMethods()
           .Single(m => m.Name == name && m.IsGenericMethod);
  }
}