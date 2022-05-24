using LinqToWql.Infrastructure;
using LinqToWql.Test.Mocks;
using LinqToWql.Test.Mocks.Resources;
using LinqToWql.Test.Mocks.Stubs;

namespace LinqToWql.Test.Infrastructure;

public class WqlResourceContextTest {
  [Fact]
  public void Ctor_ReplacesGetterForProperties_WhenPropertiesAreResources() {
    var sut = new StubWqlContext(new StubWqlContextOptions(null));

    sut.SmsCollection.Should().NotBeNull();
    sut.SmsCollection.Should().BeOfType<WqlResource<SmsCollection>>();
  }

  [Fact]
  public void CreateInstance_CreatesNewInstanceOfWqlResource_WhenTypeIsWqlResource() {
    var resultObject = new MockResultObjectBuilder<SmsCollection>(() => new SmsCollection()).Build();
    var sut = new StubWqlContext(new StubWqlContextOptions(resultObject));

    var collection = sut.CreateResourceInstance<SmsCollection>();

    collection.Should().BeOfType<SmsCollection>();
  }
}