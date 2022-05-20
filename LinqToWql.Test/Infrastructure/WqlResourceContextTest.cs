using LinqToWql.Infrastructure;
using LinqToWql.Test.Mocks;
using LinqToWql.Test.Mocks.Stubs;

namespace LinqToWql.Test.Infrastructure;

public class WqlResourceContextTest {
  [Fact]
  public void Ctor_ReplacesGetterForProperties_WhenPropertiesAreResources() {
    var sut = new StubWqlContext(new StubWqlContextOptions());

    sut.SmsCollection.Should().NotBeNull();
    sut.SmsCollection.Should().BeOfType<WqlResource<SmsCollection>>();
  }
}