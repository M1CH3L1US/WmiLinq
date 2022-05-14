using System.Linq.Expressions;

namespace LinqToWql.Language.Expressions;

public class BinaryWqlExpression : WqlExpression {
  public WqlExpression Left;
  public ExpressionType Operator;
  public WqlExpression Right;

  public BinaryWqlExpression(WqlExpression left, ExpressionType @operator, WqlExpression right) {
    Left = left;
    Right = right;
    Operator = @operator;
  }

  private string GetOperatorAsString() {
    return Operator switch {
      ExpressionType.Equal => "=",
      ExpressionType.NotEqual => "!=",
      ExpressionType.GreaterThan => ">",
      ExpressionType.GreaterThanOrEqual => ">=",
      ExpressionType.LessThan => "<",
      ExpressionType.LessThanOrEqual => "<=",
      ExpressionType.AndAlso => "AND",
      ExpressionType.OrElse => "OR",
      _ => throw new NotSupportedException("The specified operator is not supported.")
    };
  }

  public override string ToWqlString() {
    var op = GetOperatorAsString();
    return $"{Left.ToWqlString()} {op} {Right.ToWqlString()}";
  }
}