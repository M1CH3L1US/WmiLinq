using LinqToWql.Model;

namespace LinqToWql.Language;

public static class WqlResourcePropertyQueryExtensions {
  public static bool Like<TSource, TComparison>(this WqlResourceProperty<TSource> property, TComparison value) {
    throw new NotSupportedException();
  }

  public static bool IsA<TSource>(this WqlResourceProperty<TSource> property, string type) {
    throw new NotSupportedException();
  }
}