using LinqToWql.Infrastructure;
using LinqToWql.Test.Mocks;
using LinqToWql.Test.Mocks.Resources;
using LinqToWql.Test.Mocks.Stubs;

namespace LinqToWql.Test.Infrastructure;

public class WqlResourceContextTest {
  private StubWqlResourceContext _sut = new ResourceContextBuilder()
    .ConfigureConnection()
    .DefineCreateInstance(() => new SmsCollection())
    .Complete()
    .Build();

  [Fact]
  public void Ctor_ReplacesGetterForProperties_WhenPropertiesAreResources() {
    _sut.SmsCollection.Should().NotBeNull();
    _sut.SmsCollection.Should().BeOfType<WqlResource<SmsCollection>>();
  }

  [Fact]
  public void CreateInstance_CreatesNewInstanceOfWqlResource_WhenTypeIsWqlResource() {
    var collection = _sut.CreateResourceInstance<SmsCollection>();

    collection.Should().BeOfType<SmsCollection>();
  }
}