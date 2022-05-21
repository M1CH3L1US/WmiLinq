using LinqToWql.Infrastructure;
using LinqToWql.Language;
using LinqToWql.Language.Statements;
using LinqToWql.Test.Mocks;

namespace LinqToWql.Test.Language;

public class WqlExpressionVisitorTest {
  private static readonly WqlResource<SmsCollection> _resource = StubResourceFactory.Create<SmsCollection>();

  private static readonly WqlExpressionVisitor _sut = new();

  [Fact]
  public void TranslateSelect_CreatesSelectSingleWqlExpression_WhenSelectHasSingleProperty() {
    var query = _resource.Select(c => c.Name);

    var result = _sut.Visit(query.Expression);

    result.Should().BeAssignableTo<SelectWqlStatement>();
  }

  [Fact]
  public void TranslateSelect_CreatesSelectMultipleWqlExpression_WhenSelectHasAnonymousObject() {
    var query = _resource.Select(c => new {c.Name, c.Description});

    var result = _sut.Visit(query.Expression);

    result.Should().BeAssignableTo<SelectWqlStatement>();
  }

  [Fact]
  public void TranslateSelect_CreatesSelectWithTwoNamedProperties_WhenSelectHasAnonymousObject() {
    var query = _resource.Select(c => new {c.Name, c.Description});
    ;

    var result = (SelectWqlStatement) _sut.Visit(query.Expression);

    result.SelectProperties.Count.Should().Be(2);
    result.SelectProperties.Should().Contain(c => c.PropertyName == "Name");
    result.SelectProperties.Should().Contain(c => c.PropertyName == "Description");
  }

  [Fact]
  public void TranslateWhere_TranslatesNestedWhereConditions() {
    var query = _resource.Where(x => x.Name == "test" && x.Description == "test");

    var result = (WhereWqlExpression) _sut.Visit(query.Expression);
    var str = result.InnerExpression.ToWqlString();

    str.Should().Be(@"Name = ""test"" AND Description = ""test""");
  }
}