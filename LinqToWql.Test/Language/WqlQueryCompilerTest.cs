using System.Collections;
using LinqToWql.Infrastructure;
using LinqToWql.Language;
using LinqToWql.Test.Mocks;
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

    var context = new Mock<IWqlResourceContext>();
    context.Setup(c => c.InvokeQuery(It.IsAny<string>()));
    var sut = new WqlQueryRunner(context.Object);

    sut.Execute<IEnumerable>(q.Expression);

    var arg = context.Invocations.First().Arguments.First();

    arg.Should().Be("SELECT Name, Description"
                    + NewLine +
                    "FROM SMS_Collection"
                    + NewLine +
                    "WHERE Name = \"Test\""
                    + NewLine +
                    "OR CollectionID = \"1-100-10\" AND Description = \"Test\""
                    + NewLine);
  }
}