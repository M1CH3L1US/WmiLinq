using LinqToWql.Infrastructure;
using LinqToWql.Language;
using LinqToWql.Language.Statements;
using LinqToWql.Test.Mocks;
using LinqToWql.Test.Mocks.Resources;

namespace LinqToWql.Test.Language;

public class WqlExpressionVisitorTest {
  private static readonly WqlResource<SmsCollection> _resource =
    MockResourceFactory<SmsCollection>.CreateWithResultValue(() => new SmsCollection());

  private static readonly WqlExpressionVisitor _sut = new();

  [Fact]
  public void TranslateSelect_CreatesSelectSingleWqlExpression_WhenSelectHasSingleProperty() {
    var query = _resource.Select(c => c.Name);

    var result = _sut.Visit(query.Expression);

    result.Should().BeAssignableTo<WqlStatement>();
  }

  [Fact]
  public void TranslateSelect_CreatesSelectMultipleWqlExpression_WhenSelectHasAnonymousObject() {
    var query = _resource.Select(c => new {c.Name, c.Description});

    var result = _sut.Visit(query.Expression);

    result.Should().BeAssignableTo<WqlStatement>();
  }
}