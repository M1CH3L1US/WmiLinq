using System.Collections;

namespace LinqToWql.Model;

public readonly struct WqlResourceProperty<T> : IEnumerable<T> {
  public bool Equals(WqlResourceProperty<T> other) {
    return EqualityComparer<T>.Default.Equals(Value, other.Value);
  }

  public IEnumerator<T> GetEnumerator() {
    if (Value is IEnumerable<T> enumerable) {
      return enumerable.GetEnumerator();
    }

    return ValueIterator();
  }

  private IEnumerator<T> ValueIterator() {
    yield return Value;
  }

  public override bool Equals(object? obj) {
    return obj is WqlResourceProperty<T> other && Equals(other);
  }

  public override int GetHashCode() {
    return EqualityComparer<T>.Default.GetHashCode(Value);
  }

  IEnumerator IEnumerable.GetEnumerator() {
    return GetEnumerator();
  }

  public T Value { get; }

  public WqlResourceProperty(T value) {
    Value = value;
  }

  public static implicit operator WqlResourceProperty<T>(T value) {
    return new WqlResourceProperty<T>(value);
  }

  public static implicit operator T(WqlResourceProperty<T> instance) {
    return instance.Value;
  }

  public static bool operator ==(WqlResourceProperty<T> @this, T compare) {
    return @this.Value?.Equals(compare) ?? false;
  }

  public static bool operator !=(WqlResourceProperty<T> @this, T compare) {
    return !@this.Value?.Equals(compare) ?? true;
  }

  public static bool operator >(WqlResourceProperty<T> @this, T compare) {
    return ThrowNotSupported();
  }

  public static bool operator <(WqlResourceProperty<T> @this, T compare) {
    return ThrowNotSupported();
  }

  public static bool operator <=(WqlResourceProperty<T> @this, T compare) {
    return ThrowNotSupported();
  }

  public static bool operator >=(WqlResourceProperty<T> @this, T compare) {
    return ThrowNotSupported();
  }

  private static bool ThrowNotSupported() {
    throw new NotSupportedException("These operators are not supported at runtime. They merely serve as a utility" +
                                    "when using them as part of a query builder");
  }
}