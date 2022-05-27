using LinqToWql.Data;
using LinqToWql.Infrastructure;
using LinqToWql.Test.Mocks;
using LinqToWql.Test.Mocks.Resources;
using LinqToWql.Test.Mocks.Stubs;
using Moq;

namespace LinqToWql.Test.Infrastructure;

public class WqlResourceContextTest {
  private readonly StubWqlResourceContext _sut = new ResourceContextBuilder()
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
  public void CreateResourceInstance_CreatesNewInstanceOfWqlResource_WhenTypeIsWqlResource() {
    var collection = _sut.CreateResourceInstance<SmsCollection>();

    collection.Should().BeAssignableTo<SmsCollection>();
  }

  [Fact]
  public void CreateResourceInstance_CreatesNewInstanceOfWqlResource_WhenTypeIsWqlResourceBase() {
    var mockResource = new Mock<IResourceObject>();
    var collection = _sut.CreateResourceInstance<ISmsCollectionRule>(mockResource.Object);

    collection.Should().BeAssignableTo<ISmsCollectionRule>();
  }
}