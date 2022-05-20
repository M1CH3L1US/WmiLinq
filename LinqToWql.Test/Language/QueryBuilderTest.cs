using System.Linq.Expressions;
using LinqToWql.Infrastructure;
using LinqToWql.Language;
using LinqToWql.Language.Expressions;
using LinqToWql.Language.Statements;
using LinqToWql.Test.Mocks;

namespace LinqToWql.Test.Language;

public class QueryBuilderTest {
  private const string ResourceName = "SMS_Collection";
  private const string NewLine = "\r\n";

  private static readonly WqlResource<SmsCollection> _resource = StubResourceFactory.Create<SmsCollection>();
  private static readonly Expression _root = Expression.Constant(_resource);

  [Fact]
  public void GetResourceName_GetsTheResourceNameFromTheQuery() {
    var expressionTree = new EmptyWqlStatement(_root);
    var sut = new WqlQueryBuilder(expressionTree);

    // We need to traverse the tree first
    // to find the resource
    sut.TraverseQueryTree();
    var resourceName = sut.GetSelectedResourceName();

    resourceName.Should().Be(ResourceName);
  }

  [Fact]
  public void AppendSelect_CreatesQueryWithSelectStart_WhenQueryIsEmpty() {
    var expressionTree = new EmptyWqlStatement(_root);
    var sut = new WqlQueryBuilder(expressionTree);
    var result = sut.Build(out _);

    result.Should().Be("SELECT *"
                       + NewLine +
                       $"FROM {ResourceName}"
                       + NewLine);
  }

  [Fact]
  public void AppendSelect_CreatesQueryWithSelectSingleProperty_WhenQueryHasSingleSelectSingleProperty() {
    var expressionTree = new SelectWqlStatement(_root, new List<PropertyWqlExpression> {new("Name")});

    var sut = new WqlQueryBuilder(expressionTree);
    var result = sut.Build(out _);

    result.Should().Be("SELECT Name"
                       + NewLine +
                       $"FROM {ResourceName}"
                       + NewLine);
  }

  [Fact]
  public void AppendSelect_CreatesQueryWithSelectMultipleProperties_WhenQueryHasSingleSelectMultipleProperties() {
    var expressionTree = new SelectWqlStatement(_root, new List<PropertyWqlExpression> {
      new("Name"),
      new("Description")
    });

    var sut = new WqlQueryBuilder(expressionTree);
    var result = sut.Build(out _);

    result.Should().Be("SELECT Name, Description"
                       + NewLine +
                       $"FROM {ResourceName}"
                       + NewLine);
  }

  [Fact]
  public void AppendSelect_UsesTheLastSelectStatementInTheExpressionTree_WhenTreeHasMultipleSelectExpressions() {
    var expressionTree = new SelectWqlStatement(
      new SelectWqlStatement(
        _root,
        new List<PropertyWqlExpression> {new("Name"), new("Description")}
      ),
      new List<PropertyWqlExpression> {new("Name")}
    );

    var sut = new WqlQueryBuilder(expressionTree);
    var result = sut.Build(out _);

    result.Should().Be("SELECT Name"
                       + NewLine +
                       $"FROM {ResourceName}"
                       + NewLine);
  }

  [Fact]
  public void AppendWhere_CreatesQueryWithPropertyToConstantComparison_WhenTreeHasSingleWhereWithConstantComparison() {
    var comparison = new BinaryWqlExpression(
      new PropertyWqlExpression("Name"),
      ExpressionType.Equal,
      new ConstantWqlExpression("Test")
    );
    var expressionTree = new WhereWqlExpression(_root, comparison);

    var sut = new WqlQueryBuilder(expressionTree);
    var result = sut.Build(out _);

    result.Should().Be("SELECT *"
                       + NewLine +
                       $"FROM {ResourceName}"
                       + NewLine +
                       "WHERE Name = \"Test\""
                       + NewLine);
  }

  [Fact]
  public void AppendWhere_CreatesQueryWithAndChainedWhereClause_WhenTreeHasMultipleWhereClauses() {
    var clause1 = new BinaryWqlExpression(
      new PropertyWqlExpression("Name"),
      ExpressionType.Equal,
      new ConstantWqlExpression("Test")
    );

    var clause2 = new BinaryWqlExpression(
      new PropertyWqlExpression("Description"),
      ExpressionType.Equal,
      new ConstantWqlExpression("Test")
    );

    var expressionTree = new WhereWqlExpression(new WhereWqlExpression(_root, clause2), clause1);

    var sut = new WqlQueryBuilder(expressionTree);
    var result = sut.Build(out _);

    result.Should().Be("SELECT *"
                       + NewLine +
                       $"FROM {ResourceName}"
                       + NewLine +
                       "WHERE Name = \"Test\""
                       + NewLine +
                       "AND Description = \"Test\""
                       + NewLine);
  }

  [Fact]
  public void AppendWhere_CreatesQueryWithOrChainedWhereClause_WhenTreeHasMultipleWhereClausesWithOr() {
    var clause1 = new BinaryWqlExpression(
      new PropertyWqlExpression("Name"),
      ExpressionType.Equal,
      new ConstantWqlExpression("Test")
    );

    var clause2 = new BinaryWqlExpression(
      new PropertyWqlExpression("Description"),
      ExpressionType.Equal,
      new ConstantWqlExpression("Test")
    );

    var expressionTree =
      new WhereWqlExpression(new WhereWqlExpression(_root, clause2), clause1, ExpressionChainType.Or);

    var sut = new WqlQueryBuilder(expressionTree);
    var result = sut.Build(out _);

    result.Should().Be("SELECT *"
                       + NewLine +
                       $"FROM {ResourceName}"
                       + NewLine +
                       "WHERE Name = \"Test\""
                       + NewLine +
                       "OR Description = \"Test\""
                       + NewLine);
  }
}