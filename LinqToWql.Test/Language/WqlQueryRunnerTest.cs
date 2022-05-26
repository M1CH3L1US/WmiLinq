using LinqToWql.Infrastructure;
using LinqToWql.Language;
using LinqToWql.Test.Mocks;
using LinqToWql.Test.Mocks.Resources;
using LinqToWql.Test.Mocks.Stubs;

namespace LinqToWql.Test.Language;

public class WqlQueryRunnerTest {
  private const string NewLine = "\r\n";
  private WqlResource<SmsCollection> _resource = new ResourceContextBuilder().BuildForResource<SmsCollection>();
  /*
  [Fact]
  public void LikeQuery_IsParsedCorrectly_WhenQueryContainsLikeOperation() {
    var expression = _resource.Where(c => c.Name.Like("%Foo%"))
                              .Expression;

    var sut = MakeQueryRunner(out var queryProcessor);
    sut.Execute<IEnumerable<SmsCollection>>(expression);

    var arg = queryProcessor.LastQuery;

    arg.Should().Be("SELECT *"
                    + NewLine +
                    "FROM SMS_Collection"
                    + NewLine +
                    "WHERE Name LIKE \"%Foo%\""
                    + NewLine);
  }
  */
}