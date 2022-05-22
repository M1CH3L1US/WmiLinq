using LinqToWql.Model;

namespace LinqToWql.Language;

public static class WqlResourcePropertyQueryExtensions {
  public static bool Like<TSource, TComparison>(this WqlResourceProperty<TSource> property, TComparison value) {
    throw new NotSupportedException($"${nameof(Like)} is only supported within .Where and .OrWhere statements");
  }

  public static bool IsA<TSource>(this WqlResourceProperty<TSource> property, string type) {
    throw new NotSupportedException($"${nameof(IsA)} is only supported within .Where and .OrWhere statements");
  }
}