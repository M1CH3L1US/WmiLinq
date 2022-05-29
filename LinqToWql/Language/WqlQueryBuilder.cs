using System.Linq.Expressions;
using System.Text;
using LinqToWql.Infrastructure;
using LinqToWql.Language.Expressions;
using LinqToWql.Language.Statements;

namespace LinqToWql.Language;

public class WqlQueryBuilder {
  private readonly StringBuilder _query = new();

  /// <summary>
  ///   We use a HashSet to only have unique select
  ///   properties.
  /// </summary>
  private readonly HashSet<string> _selectProperties = new();

  private readonly WqlStatement _source;
  private readonly List<WqlStatement> _statements = new();

  private readonly Dictionary<string, ExpressionChainType> _whereClauses = new();

  public readonly QueryResultParseOptions ParseOptions;

  public ConstantExpression WqlResourceExpression { get; set; }

  /// <summary>
  ///   The runtime type of the WqlResource
  /// </summary>
  public Type WqlResourceType => WqlResourceExpression.Value
                                                      .GetType()
                                                      .GetGenericArguments()
                                                      .Single();

  public WqlQueryBuilder(WqlStatement source, WqlResourceContext context) {
    _source = source;
    ParseOptions = new QueryResultParseOptions(context);
  }

  public string Build(out QueryResultParseOptions parseOptions) {
    BuildInternal();
    ApplyParseOptions();
    parseOptions = ParseOptions;
    return _query.ToString();
  }

  public void AddWhereClause(WqlExpression expression, ExpressionChainType chainType = ExpressionChainType.And) {
    _whereClauses.Add(expression.ToWqlString(), chainType);
  }

  public void AddSelectProperties(IList<PropertyWqlExpression> properties) {
    var propertyNames = properties.Select(p => p.PropertyName);
    foreach (var propertyName in propertyNames) {
      _selectProperties.Add(propertyName);
    }
  }

  private void ApplyParseOptions() {
    ParseOptions.ResourceType = WqlResourceType;
  }

  private void BuildInternal() {
    TraverseQueryTree();
    ParseStatements();
    AppendSelect();
    AppendFrom();
    AppendWhereToQuery();
  }

  private void ParseStatements() {
    foreach (var statement in _statements) {
      statement.AppendSelfToQuery(this);
    }
  }

  internal void TraverseQueryTree() {
    Expression statement = _source;

    while (statement is WqlStatement wqlStatement) {
      _statements.Add(wqlStatement);
      statement = wqlStatement.InnerStatement;
    }

    // By traversing the tree inwards, we get the statements
    // in reversed order than how they were originally created in.
    // To accurately reflect things like OR where chains, we reverse
    // the statement tree again.
    _statements.Reverse();

    // The first element in an expression tree will always be the query source
    // in the form of a constant expression. 
    WqlResourceExpression = (ConstantExpression) statement!;
  }

  private void AppendWhereToQuery() {
    if (_whereClauses.Count == 0) {
      return;
    }

    var isFirstWhereClause = true;
    _query.Append("WHERE ");

    foreach (var whereClause in _whereClauses) {
      var clause = whereClause.Key;
      var chainType = whereClause.Value;

      // Don't append the chain operator on the first clause
      if (!isFirstWhereClause) {
        var chainTypeString = chainType.ToString().ToUpper();
        _query.Append(chainTypeString);
        _query.Append(' ');
      }
      else {
        isFirstWhereClause = false;
      }

      _query.AppendLine(clause);
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
    if (_selectProperties.Count == 0) {
      _query.AppendLine("*");
      return;
    }

    var propertyString = string.Join(", ", _selectProperties);

    _query.AppendLine(propertyString);
  }
}