using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace LinqToWql.Language.Expressions; 

public class IsAWqlExpression : WqlExpression {
  public string ComparisonType;
  public string PropertyName;
  
  public IsAWqlExpression(string property, string comparisonType) {
    PropertyName = property;
    ComparisonType = comparisonType;
  }

  public override string ToWqlString() {
    return $@"{PropertyName} ISA ""{ComparisonType}""";
  }
}