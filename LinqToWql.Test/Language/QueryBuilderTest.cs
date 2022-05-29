using System.Linq.Expressions;
using LinqToWql.Infrastructure;
using LinqToWql.Language;
using LinqToWql.Language.Expressions;
using LinqToWql.Test.Mocks;
using LinqToWql.Test.Mocks.Resources;
using Moq;

namespace LinqToWql.Test.Language;

public class QueryBuilderTest {
  private const string ResourceName = "SMS_Collection";
  private const string NewLine = "\r\n";

  private static readonly WqlResource<SmsCollection> _resource = MockResourceFactory.CreateEmpty<SmsCollection>();

  private static readonly WqlResourceContext _context =
    new Mock<WqlResourceContext>(new Mock<WqlContextOptions>(new Mock<IWqlConnection>().Object).Object).Object;

  private static readonly Expression _root = Expression.Constant(_resource);

  [Fact]
  public void GetResourceName_GetsTheResourceNameFromTheQuery() {
    var expressionTree = new WqlStatementBuilder(_root).Build();
    var sut = new WqlQueryBuilder(expressionTree, _context);

    // We need to traverse the tree first
    // to find the resource
    sut.TraverseQueryTree();
    var resourceName = sut.GetSelectedResourceName();

    resourceName.Should().Be(ResourceName);
  }

  [Fact]
  public void AppendSelect_CreatesQueryWithSelectStart_WhenQueryIsEmpty() {
    var expressionTree = new WqlStatementBuilder(_root).Build();
    var sut = new WqlQueryBuilder(expressionTree, _context);
    var result = sut.Build(out _);

    result.Should().Be("SELECT *"
                       + NewLine +
                       $"FROM {ResourceName}"
                       + NewLine);
  }

  [Fact]
  public void AppendSelect_CreatesQueryWithSelectSingleProperty_WhenQueryHasSingleSelectSingleProperty() {
    var expressionTree = new WqlStatementBuilder(_root)
                         .AddSelectClauseFromLambda(
                           Lambda<SmsCollection>(x => x.Name))
                         .Build();

    var sut = new WqlQueryBuilder(expressionTree, _context);
    var result = sut.Build(out _);

    result.Should().Be("SELECT Name"
                       + NewLine +
                       $"FROM {ResourceName}"
                       + NewLine);
  }

  [Fact]
  public void AppendSelect_CreatesQueryWithSelectMultipleProperties_WhenQueryHasSingleSelectMultipleProperties() {
    var expressionTree = new WqlStatementBuilder(_root)
                         .AddSelectClauseFromLambda(
                           Lambda<SmsCollection>(x => new {x.Name, x.Description}))
                         .Build();

    var sut = new WqlQueryBuilder(expressionTree, _context);
    var result = sut.Build(out _);

    result.Should().Be("SELECT Name, Description"
                       + NewLine +
                       $"FROM {ResourceName}"
                       + NewLine);
  }

  [Fact]
  public void AppendSelect_UsesTheLastSelectStatementInTheExpressionTree_WhenTreeHasMultipleSelectExpressions() {
    var nestedSelectStatement = new WqlStatementBuilder(_root)
                                .AddSelectClauseFromLambda(Lambda<SmsCollection>(x => new {x.Name, x.Description}))
                                .Build();

    var expressionTree = new WqlStatementBuilder(nestedSelectStatement)
                         .AddSelectClauseFromLambda(
                           Lambda<SmsCollection>(x => x.Name))
                         .Build();

    var sut = new WqlQueryBuilder(expressionTree, _context);
    var result = sut.Build(out _);

    result.Should().Be("SELECT Name, Description"
                       + NewLine +
                       $"FROM {ResourceName}"
                       + NewLine);
  }

  [Fact]
  public void AppendWhere_CreatesQueryWithPropertyToConstantComparison_WhenTreeHasSingleWhereWithConstantComparison() {
    var expressionTree = new WqlStatementBuilder(_root)
                         .AddWhereClauseFromLambda(Lambda<SmsCollection>(x => x.Name == "Test"))
                         .Build();

    var sut = new WqlQueryBuilder(expressionTree, _context);
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
    var innerWhereStatement = new WqlStatementBuilder(_root)
                              .AddWhereClauseFromLambda(Lambda<SmsCollection>(x => x.Name == "Test"))
                              .Build();

    var expressionTree = new WqlStatementBuilder(innerWhereStatement)
                         .AddWhereClauseFromLambda(Lambda<SmsCollection>(x => x.Description == "Test"))
                         .Build();

    var sut = new WqlQueryBuilder(expressionTree, _context);
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
    var innerWhereStatement = new WqlStatementBuilder(_root)
                              .AddWhereClauseFromLambda(Lambda<SmsCollection>(x => x.Name == "Test"))
                              .Build();

    var expressionTree = new WqlStatementBuilder(innerWhereStatement)
                         .AddWhereClauseFromLambda(Lambda<SmsCollection>(x => x.Description == "Test"),
                           ExpressionChainType.Or)
                         .Build();

    var sut = new WqlQueryBuilder(expressionTree, _context);
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

  [Fact]
  public void AppendWhere_CreatesQueryWithLikeClause_WhenWhereHasLikeClause() {
    var expressionTree = new WqlStatementBuilder(_root)
                         .AddWhereClauseFromLambda(Lambda<SmsCollection>(x => x.Name.Like("%Foo%")))
                         .Build();

    var sut = new WqlQueryBuilder(expressionTree, _context);
    var result = sut.Build(out _);

    result.Should().Be("SELECT *"
                       + NewLine +
                       $"FROM {ResourceName}"
                       + NewLine +
                       "WHERE Name LIKE \"%Foo%\""
                       + NewLine);
  }

  [Fact]
  public void AppendWhere_CreatesWhereClauseWithClosureValue_WhenLambdaContainsClosureValue() {
    var closureValue = "Foo";
    var expressionTree = new WqlStatementBuilder(_root)
                         .AddWhereClauseFromLambda(Lambda<SmsCollection>(x => x.Name == closureValue))
                         .Build();

    var sut = new WqlQueryBuilder(expressionTree, _context);
    var result = sut.Build(out _);

    result.Should().Be("SELECT *"
                       + NewLine +
                       $"FROM {ResourceName}"
                       + NewLine +
                       "WHERE Name = \"Foo\""
                       + NewLine);
  }

  [Fact]
  public void AppendWhere_CreatesValueQuery_WhenLambdaContainsClosureValueOfTypeWqlResourceProperty() {
    var closureValue = "Foo";
    var expressionTree = new WqlStatementBuilder(_root)
                         .AddWhereClauseFromLambda(Lambda<SmsCollection>(x => x.Name == closureValue))
                         .Build();

    var sut = new WqlQueryBuilder(expressionTree, _context);
    var result = sut.Build(out _);

    result.Should().Be("SELECT *"
                       + NewLine +
                       $"FROM {ResourceName}"
                       + NewLine +
                       "WHERE Name = \"Foo\""
                       + NewLine);
  }

  private LambdaExpression Lambda<T>(Expression<Func<T, object>> lambda) {
    if (lambda.Body is UnaryExpression) {
      return Expression.Lambda(((UnaryExpression) lambda.Body).Operand);
    }

    return lambda;
  }
}