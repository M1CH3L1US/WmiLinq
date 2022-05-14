using LinqToWql.Language;
using LinqToWql.Language.Expressions;
using LinqToWql.Test.Mocks;

namespace LinqToWql.Test.Language; 

public class WqlExpressionVisitorTest {
  private static WqlResource<SmsCollection> _resource = StubResourceFactory.Create<SmsCollection>();
  private static WqlExpressionVisitor _sut = new ();

  [Fact]
  public void TranslateSelect_CreatesSelectSingleWqlExpression_WhenSelectHasSingleProperty() {
    var query = _resource.Select(c => c.Name);

    var result = _sut.Visit(query.Expression);

    result.Should().BeAssignableTo<SelectWqlExpression>();
  }
  
  [Fact]
  public void TranslateSelect_CreatesSelectMultipleWqlExpression_WhenSelectHasAnonymousObject() {
    var query = _resource.Select(c => new { c.Name, c.Description });;

    var result = _sut.Visit(query.Expression);

    result.Should().BeAssignableTo<SelectWqlExpression>();
  }
  
  [Fact]
  public void TranslateSelect_CreatesSelectWithTwoNamedProperties_WhenSelectHasAnonymousObject() {
    var query = _resource.Select(c => new { c.Name, c.Description });;

    var result = (SelectWqlExpression) _sut.Visit(query.Expression);

    result.SelectProperties.Count.Should().Be(2);
    result.SelectProperties.Should().Contain(c => c.PropertyName == "Name");
    result.SelectProperties.Should().Contain(c => c.PropertyName == "Description");
  }
}