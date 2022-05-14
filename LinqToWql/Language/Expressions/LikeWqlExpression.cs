using System.Linq.Expressions;

namespace LinqToWql.Language.Expressions; 

public class LikeWqlExpression : WqlExpression {
  public string Pattern;
  public string Match;

  public LikeWqlExpression(string match, string pattern) {
    Match = match;
    Pattern = pattern;
  }

  public override string ToWqlString() {
    return $@"{Match} LIKE ""{Pattern}""";
  }
}