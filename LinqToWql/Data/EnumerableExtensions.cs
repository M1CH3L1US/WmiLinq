using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LinqToWql.Data;

public static class EnumerableExtensions {
  public static bool IsEnumerable(this Type type, out Type? enumerableType) {
    enumerableType = null;

    if (!type.IsGenericType)
    {
      return false;
    }

    if (type.GetGenericTypeDefinition() != typeof(IEnumerable<>))
    {
      return false;
    }

    enumerableType = type.GetGenericArguments().First();
    return true;
  }

  /// <summary>
  /// Casts the input IEnumerable to 
  /// IEnumerable[<see cref="type">].
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="enumerable"></param>
  /// <param name="type"></param>
  /// <returns></returns>
  public static object RuntimeCast(this IEnumerable<object> enumerable, Type type) {
    var genericCastMethod = typeof(Enumerable)
                           .GetMethod(nameof(Enumerable.Cast))!
                           .MakeGenericMethod(type);

    return genericCastMethod.Invoke(null, new object[] { enumerable })!;
  }

  public static IEnumerable<object> Empty(Type type) {
    var genericEmptyMethod = typeof(Enumerable)
      .GetMethod(nameof(Enumerable.Empty))
      .MakeGenericMethod(type);

    return (IEnumerable<object>) genericEmptyMethod.Invoke(null, new object[] { });
  }
}
