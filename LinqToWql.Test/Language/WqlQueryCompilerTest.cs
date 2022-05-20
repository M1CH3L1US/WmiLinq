using LinqToWql.Infrastructure;
using LinqToWql.Language;
using LinqToWql.Test.Mocks;
using LinqToWql.Test.Mocks.Stubs;
using Moq;

namespace LinqToWql.Test.Language;

public class WqlQueryCompilerTest {
  private const string NewLine = "\r\n";

  private static readonly WqlResource<SmsCollection> _resource = StubResourceFactory.Create<SmsCollection>();

  [Fact]
  public void Test_ExampleQuery() {
    var q = _resource.Where(c => c.CollectionId == "1-100-10" && c.Description == "Test")
                     .OrWhere(c => c.Name == "Test")
                     .Select(c => new {c.Name, c.Description});

    var processor = new Mock<IWqlQueryProcessor>();
    // context.Setup(c => c.InvokeQuery(It.IsAny<string>()));
    var options = new StubWqlContextOptions();
    options.WqlQueryProcessor = processor.Object;
    var context = new StubWqlResourceContext(options);
    var sut = new WqlQueryRunner(context);

    sut.Execute<SmsCollection>(q.Expression);

    var arg = processor.Invocations.First().Arguments.First();

    arg.Should().Be("SELECT Name, Description"
                    + NewLine +
                    "FROM SMS_Collection"
                    + NewLine +
                    "WHERE Name = \"Test\""
                    + NewLine +
                    "OR CollectionId = \"1-100-10\" AND Description = \"Test\""
                    + NewLine);
  }
}