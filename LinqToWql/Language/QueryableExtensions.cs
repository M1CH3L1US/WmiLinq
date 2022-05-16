using System.Linq.Expressions;
using System.Reflection;

namespace LinqToWql.Language;

public static class QueryableExtensions {
  /// <summary>
  ///   A polyfill property for the WQL __CLASS Identifier.
  ///   https://docs.microsoft.com/en-us/windows/win32/wmisdk/--class-identifier
  /// </summary>
  // ReSharper disable once InconsistentNaming
  public static readonly object __CLASS = new StubToString("__CLASS");
  
  /// <summary>
  ///   https://docs.microsoft.com/en-us/windows/win32/wmisdk/within-clause
  /// </summary>
  /// <param name="source"></param>
  /// <param name="timeoutInSeconds"></param>
  /// <typeparam name="TSource"></typeparam>
  public static IQueryable<TSource> Within<TSource>(
    this IQueryable<TSource> source,
    int timeoutInSeconds
  ) {
    return source.Provider.CreateQuery<TSource>(
      Expression.Call(
        null,
        GetMethodInfo(Within, source, timeoutInSeconds),
        new[] {source.Expression, Expression.Constant(timeoutInSeconds)}
      ));
  }

  /// <summary>
  ///   https://docs.microsoft.com/en-us/windows/win32/wmisdk/having-clause
  /// </summary>
  /// <param name="source"></param>
  /// <param name="predicate"></param>
  /// <typeparam name="TSource"></typeparam>
  public static IQueryable<TSource> Having<TSource>(
    this IQueryable<TSource> source,
    Expression<Func<TSource, bool>> predicate
  ) {
    return source.Provider.CreateQuery<TSource>(
      Expression.Call(
        null,
        GetMethodInfo(Having, source, predicate),
        new[] {source.Expression, Expression.Quote(predicate)}
      ));
  }

  /// <summary>
  ///   Chains the previous and next where clause
  ///   with an OR operator instead of AND.
  /// </summary>
  /// <param name="source"></param>
  /// <typeparam name="TSource"></typeparam>
  public static IQueryable<TSource> OrWhere<TSource>(
    this IQueryable<TSource> source,
    Expression<Func<TSource, bool>> predicate
  ) {
    return source.Provider.CreateQuery<TSource>(
      Expression.Call(
        null,
        GetMethodInfo(OrWhere, source, predicate),
        new[] {source.Expression, Expression.Quote(predicate)}
      ));
  }

  #region Helper methods to obtain MethodInfo in a safe way

  // https://github.com/microsoft/referencesource/blob/master/System.Core/System/Linq/IQueryable.cs  
  private static MethodInfo GetMethodInfo<T1, T2>(Func<T1, T2> f, T1 unused1) {
    return f.Method;
  }

  private static MethodInfo GetMethodInfo<T1, T2, T3>(Func<T1, T2, T3> f, T1 unused1, T2 unused2) {
    return f.Method;
  }

  #endregion
}

internal struct StubToString {
  private readonly string _stringValue;

  public StubToString(string stringValue = "") {
    _stringValue = stringValue;
  }
  
  public override string ToString() {
    return _stringValue;
  }
}