namespace LinqToWql.Model; 

public struct WqlResourceProperty<T> {
  public T Value { get; }
  
  public WqlResourceProperty(T value) {
    Value = value;
  }
  
  public static implicit operator WqlResourceProperty<T> (T value) => new (value);
  
  public static bool operator == (WqlResourceProperty<T> @this, T compare) {
    return @this.Value?.Equals(compare) ?? false;
  }

  public static bool operator != (WqlResourceProperty<T> @this, T compare) {
    return !@this.Value?.Equals(compare) ?? true;
  }

  public static bool operator >(WqlResourceProperty<T> @this, T compare) 
    => ThrowNotSupported();

  public static bool operator <(WqlResourceProperty<T> @this, T compare)
    => ThrowNotSupported();
  
  public static bool operator <=(WqlResourceProperty<T> @this, T compare)
    => ThrowNotSupported();

  public static bool operator >=(WqlResourceProperty<T> @this, T compare) 
    => ThrowNotSupported();

  private static bool ThrowNotSupported() => 
    throw new NotSupportedException("These operators are not supported at runtime. They merely serve as a utility" +
                                                                             "when using them as part of a query builder");
}