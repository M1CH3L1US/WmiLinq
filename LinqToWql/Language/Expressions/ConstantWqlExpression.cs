namespace LinqToWql.Language.Expressions;

public class ConstantWqlExpression : WqlExpression {
  public object? Value;

  public ConstantWqlExpression(object? value) {
    Value = value;
  }

  public override string ToWqlString() {
    return Value switch {
      null => "NULL",
      string => @$"""{Value}""",
      false => "FALSE",
      true => "TRUE",
      _ => Value.ToString()
    } ?? string.Empty;
  }
}