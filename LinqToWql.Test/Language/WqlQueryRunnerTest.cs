using LinqToWql.Infrastructure;
using LinqToWql.Language;
using LinqToWql.Test.Mocks;
using LinqToWql.Test.Mocks.Stubs;
using Moq;

namespace LinqToWql.Test.Language;

public class WqlQueryRunnerTest {
  private const string NewLine = "\r\n";

  private static readonly WqlResource<SmsCollection> _resource = StubResourceFactory.Create<SmsCollection>();


  [Fact]
  public void Test_ExampleQuery() {
    var q = _resource.Where(c => c.CollectionId == "1-100-10" && c.Description == "Test")
                     .OrWhere(c => c.Name == "Test")
                     .Select(c => new {c.Name, c.Description});


    var sut = MakeQueryRunner(out var queryProcessor);
    sut.Execute<SmsCollection>(q.Expression);

    var arg = queryProcessor.Invocations.First().Arguments.First();

    arg.Should().Be("SELECT Name, Description"
                    + NewLine +
                    "FROM SMS_Collection"
                    + NewLine +
                    "WHERE CollectionId = \"1-100-10\" AND Description = \"Test\""
                    + NewLine +
                    "OR Name = \"Test\""
                    + NewLine);
  }

  [Fact]
  public void LikeQuery_IsParsedCorrectly_WhenQueryContainsLikeOperation() {
    var expression = _resource.Where(c => c.Name.Like("%Foo%"))
                              .Expression;

    var sut = MakeQueryRunner(out var queryProcessor);
    sut.Execute<SmsCollection>(expression);

    var arg = queryProcessor.Invocations.First().Arguments.First();

    arg.Should().Be("SELECT *"
                    + NewLine +
                    "FROM SMS_Collection"
                    + NewLine +
                    "WHERE Name LIKE \"%Foo%\""
                    + NewLine);
  }

  private IWqlQueryRunner MakeQueryRunner(out Mock<IWqlQueryProcessor> queryProcessorMock) {
    var options = new StubWqlContextOptions();
    queryProcessorMock = new Mock<IWqlQueryProcessor>();
    options.WqlQueryProcessor = queryProcessorMock.Object;
    var context = new StubWqlResourceContext(options);
    return new WqlQueryRunner(context);
  }
}