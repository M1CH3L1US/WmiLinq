using System.Linq.Expressions;
using System.Text;

namespace LinqToWql.Language.Expressions;

public abstract class WqlExpression : Expression {
  /*
    expressionPrinter.Visit(Match);
    expressionPrinter.Append(" LIKE ");
    expressionPrinter.Visit(Pattern);
 */
  public abstract string ToWqlString();
}