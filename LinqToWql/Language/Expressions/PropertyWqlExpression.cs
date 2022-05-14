namespace LinqToWql.Language.Expressions; 

/// <summary>
/// Represents a property on a WMI object.
/// </summary>
public class PropertyWqlExpression : WqlExpression {
  public string PropertyName;

  public PropertyWqlExpression(string propertyName) {
    PropertyName = propertyName;
  }

  public override string ToWqlString() {
    return PropertyName;
  }
}