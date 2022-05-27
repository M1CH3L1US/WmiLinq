using System.Reflection;

namespace LinqToWql.Data;

public static class EnumerableExtensions {
  private static readonly Type _enumerableType = typeof(Enumerable);
  private static readonly MethodInfo _enumerableCastMethod = GetEnumerableMethodInfo(nameof(Enumerable.Cast));
  private static readonly MethodInfo _enumerableToArrayMethod = GetEnumerableMethodInfo(nameof(Enumerable.ToArray));
  private static readonly MethodInfo _enumerableToListMethod = GetEnumerableMethodInfo(nameof(Enumerable.ToList));
  private static readonly MethodInfo _enumerableEmptyMethod = GetEnumerableMethodInfo(nameof(Enumerable.Empty));

  public static bool IsEnumerable(this Type type, out Type? enumerableType) {
    enumerableType = null;

    if (!type.IsGenericType) {
      return false;
    }

    if (type.GetGenericTypeDefinition() != typeof(IEnumerable<>)) {
      return false;
    }

    enumerableType = type.GetGenericArguments().First();
    return true;
  }

  /// <summary>
  ///   Casts the input IEnumerable to
  ///   IEnumerable[<see cref="type">].
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="enumerable"></param>
  /// <param name="type"></param>
  /// <returns></returns>
  public static object RuntimeCast(this IEnumerable<object> enumerable, Type type) {
    var genericCastMethod = _enumerableCastMethod.MakeGenericMethod(type);
    return InvokeEnumerableMethod(enumerable, genericCastMethod);
  }

  /// <summary>
  ///   Casts the enumerable to <see cref="type" />
  ///   and invokes <see cref="Enumerable.ToArray{TSource}" /> on it.
  /// </summary>
  /// <param name="enumerable"></param>
  /// <param name="type"></param>
  /// <returns></returns>
  public static object RuntimeToArrayCast(this IEnumerable<object> enumerable, Type type) {
    var castedEnumerable = RuntimeCast(enumerable, type);
    var genericToArrayMethod = _enumerableToArrayMethod.MakeGenericMethod(type);
    return InvokeEnumerableMethod(castedEnumerable, genericToArrayMethod);
  }

  /// <summary>
  ///   Casts the enumerable to <see cref="type" />
  ///   and invokes <see cref="Enumerable.ToList{TSource}" /> on it.
  /// </summary>
  /// <param name="enumerable"></param>
  /// <param name="type"></param>
  /// <returns></returns>
  public static object RuntimeToListCast(this IEnumerable<object> enumerable, Type type) {
    var castedEnumerable = RuntimeCast(enumerable, type);
    var genericToListMethod = _enumerableToListMethod.MakeGenericMethod(type);
    return InvokeEnumerableMethod(castedEnumerable, genericToListMethod);
  }

  private static object InvokeEnumerableMethod(object enumerable, MethodInfo method) {
    return method.Invoke(null, new[] {enumerable});
  }

  public static IEnumerable<object> RuntimeEmpty(Type type) {
    var genericEmptyMethod = _enumerableEmptyMethod.MakeGenericMethod(type);

    return (IEnumerable<object>) genericEmptyMethod.Invoke(null, new object[] { });
  }

  private static MethodInfo GetEnumerableMethodInfo(string name) {
    return typeof(Enumerable)
      .GetMethod(name)!;
  }
}