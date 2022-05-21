using LinqToWql.Infrastructure;
using LinqToWql.Test.Mocks;

namespace LinqToWql.Test.Infrastructure;

public class WqlQueryProcessorTest {
  private static readonly WqlResource<SmsCollection> _resource = StubResourceFactory.Create<SmsCollection>();

  [Fact]
  public void Map_MapsIResultObjectToResource_WhenOutputTypeIsResource() {
    var instances = _resource.Where(r => r.Description == "Foo");
    var count = 0;

    foreach (var instance in instances) {
      count++;
      instance.Should().BeOfType<SmsCollection>();
    }

    count.Should().Be(1);
  }
}