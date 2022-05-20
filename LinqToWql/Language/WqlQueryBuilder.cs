using System.Linq.Expressions;
using System.Text;
using LinqToWql.Infrastructure;
using LinqToWql.Language.Expressions;
using LinqToWql.Language.Statements;

namespace LinqToWql.Language;

public class WqlQueryBuilder {
  private readonly StringBuilder _query = new();

  // We could make this a generic list of WqlStatements and
  // handle the string building in the WqlStatement class, but
  // this isn't worth the effort for this.
  private readonly List<SelectWqlStatement> _selectExpressions = new();
  private readonly WqlStatement _source;
  private readonly List<WhereWqlExpression> _whereExpressions = new();

  public readonly QueryResultParseOptions ParseOptions = new();

  public ConstantExpression WqlResourceExpression { get; set; }

  /// <summary>
  ///   The runtime type of the WqlResource
  /// </summary>
  public Type WqlResourceType => WqlResourceExpression.Value
                                                      .GetType()
                                                      .GetGenericArguments()
                                                      .Single();

  public WqlQueryBuilder(WqlStatement source) {
    _source = source;
  }

  public string Build(out QueryResultParseOptions parseOptions) {
    BuildInternal();
    ApplyParseOptions();
    parseOptions = ParseOptions;
    return _query.ToString();
  }

  private void ApplyParseOptions() {
    ParseOptions.ResourceType = WqlResourceType;
  }

  private void BuildInternal() {
    TraverseQueryTree();
    AppendSelect();
    AppendFrom();
    AppendWhere();
  }

  internal void TraverseQueryTree() {
    Expression statement = _source;

    while (statement is WqlStatement wqlStatement) {
      switch (wqlStatement) {
        case SelectWqlStatement selectStatement:
          _selectExpressions.Add(selectStatement);
          break;
        case WhereWqlExpression whereStatement:
          _whereExpressions.Add(whereStatement);
          break;
      }

      statement = wqlStatement.InnerStatement;
    }

    // The first element in an expression tree will always be the query source
    // in the form of a constant expression. 
    WqlResourceExpression = (ConstantExpression) statement!;
  }

  private void AppendWhere() {
    if (_whereExpressions.Count == 0) {
      return;
    }

    _query.Append("WHERE ");

    foreach (var whereExpression in _whereExpressions) {
      var str = whereExpression.InnerExpression.ToWqlString();
      _query.AppendLine(str);

      // Don't include the chain operator if this is the last expression
      if (_whereExpressions.IndexOf(whereExpression) != _whereExpressions.Count - 1) {
        var chainOperator = whereExpression.ChainType.ToString().ToUpper();
        _query.Append($"{chainOperator} ");
      }
    }
  }

  private void AppendFrom() {
    var resource = GetSelectedResourceName();
    _query.Append("FROM ");
    _query.AppendLine(resource);
  }

  internal string GetSelectedResourceName() {
    var wqlResource = WqlResourceExpression.Value!;
    // We cannot type the resource because we don't know it's generic type at runtime.
    // By getting its generic type and the `GetResourceName` method of that, we can
    // still get access to the resource name of the instance though.
    var getResourceNameMethod = wqlResource.GetType().GetMethod(nameof(WqlResource<object>.GetResourceName))!;
    var resourceName = (string) getResourceNameMethod.Invoke(wqlResource, new object[] { })!;

    return resourceName;
  }

  private void AppendSelect() {
    _query.Append("SELECT ");

    // If there is no select query in the list,
    // we select all properties
    if (_selectExpressions.Count == 0) {
      _query.AppendLine("*");
      return;
    }

    // We only care about the last select statement
    var lastSelect = _selectExpressions.First();

    if (lastSelect.SelectToSingleProperty) {
      ParseOptions.SinglePropertyToSelect = lastSelect.SelectProperties.First().PropertyName;
    }

    var properties = lastSelect.SelectProperties.Select(c => c.PropertyName);
    var propertyString = string.Join(", ", properties);

    _query.AppendLine(propertyString);
  }
}