using System.Linq.Expressions;
using System.Reflection;

namespace LinqToWql.Language;

public static class QueryableExtensions {
  public static readonly MethodInfo WithinMethodInfo = typeof(QueryableExtensions)
                                                       .GetTypeInfo()
                                                       .GetDeclaredMethods(nameof(Within))
                                                       .Single();

  public static readonly MethodInfo AssociatorsOfMethodInfo = typeof(QueryableExtensions)
                                                              .GetTypeInfo()
                                                              .GetDeclaredMethods(nameof(AssociatorsOf))
                                                              .Single();

  public static readonly MethodInfo HavingMethodInfo = typeof(QueryableExtensions)
                                                       .GetTypeInfo()
                                                       .GetDeclaredMethods(nameof(Having))
                                                       .Single();

  public static readonly MethodInfo OrMethodInfo = typeof(QueryableExtensions)
                                                   .GetTypeInfo()
                                                   .GetDeclaredMethods(nameof(Or))
                                                   .Single();

  /// <summary>
  ///   A polyfill property for the WQL __CLASS Identifier.
  ///   https://docs.microsoft.com/en-us/windows/win32/wmisdk/--class-identifier
  /// </summary>
  // ReSharper disable once InconsistentNaming
  public static readonly object __CLASS = new();
  
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
  ///   https://docs.microsoft.com/en-us/windows/win32/wmisdk/associators-of-statement
  /// </summary>
  /// <param name="source"></param>
  /// <param name="objectPath"></param>
  /// <typeparam name="TSource"></typeparam>
  // ReSharper disable once IdentifierTypo
  public static IQueryable<TSource> AssociatorsOf<TSource>(
    this IQueryable<TSource> source,
    string objectPath
  ) {
    return source.Provider.CreateQuery<TSource>(
      Expression.Call(
        null,
        GetMethodInfo(AssociatorsOf, source, objectPath),
        new[] {source.Expression, Expression.Constant(objectPath)}
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
  public static IQueryable<TSource> Or<TSource>(
    this IQueryable<TSource> source
  ) {
    return source.Provider.CreateQuery<TSource>(
      Expression.Call(
        null,
        GetMethodInfo(Or, source)
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