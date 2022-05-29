using LinqToWql.Data;
using LinqToWql.Infrastructure;
using LinqToWql.Model;
using LinqToWql.Test.Mocks;
using LinqToWql.Test.Mocks.Resources;
using LinqToWql.Test.Mocks.Stubs;
using Moq;

namespace LinqToWql.Test.Infrastructure;

public class WqlResourceProxyGeneratorTest {
  private static readonly WqlResourceContext
    _context = new Mock<WqlResourceContext>(new StubWqlContextOptions()).Object;

  private readonly WqlResourceProxyGenerator _proxyGenerator = new();

  private readonly IResourceObject _testResource = MockResourceFactory.CreateResourceObject<ISmsCollectionRule>(() =>
    new SmsCollectionRule {
      Name = "Foo",
      Id = 10,
      NonResourceProperty = "Test"
    }, _context);

  [Fact]
  public void CreateResourceInterfaceProxy_CreatesAnObjectOfTheInterfaceType_WhenTypeIsInterface() {
    var proxy = _proxyGenerator.CreateResourceInterfaceProxy<ISmsCollectionRule>(_testResource);

    proxy.Should().BeAssignableTo<ISmsCollectionRule>();
  }

  [Fact]
  public void CreateResourceInterfaceProxy_InterfaceFieldsAreProxiedToResource_WhenFieldIsGetter() {
    var sut = _proxyGenerator.CreateResourceInterfaceProxy<ISmsCollectionRule>(_testResource);

    sut.Name.Should().Be("Foo");
  }

  [Fact]
  public void CreateResourceInterfaceProxy_InterfaceFieldsAreProxiedToResource_WhenFieldIsSetter() {
    var sut = _proxyGenerator.CreateResourceInterfaceProxy<ISmsCollectionRule>(_testResource);
    sut.Name = "Bar";

    sut.Name.Should().Be("Bar");
  }

  [Fact]
  public void AdaptTo_CreatesAnInstanceOfTheAdaptedClassUsingTheResourceAsABase_WhenResourceIsAssignableToClass() {
    var sut = _proxyGenerator.CreateResourceInterfaceProxy<ISmsCollectionRule>(_testResource);
    var adapted = sut.AdaptTo<SmsCollectionRule>();

    adapted.Resource.Should().BeSameAs(_testResource);
  }

  [Fact]
  public void CreateResourceProxy_CreatesAnObjectOfTheClassType_WhenTypeIsClass() {
    var proxy = _proxyGenerator.CreateResourceProxy<SmsCollectionRule>(_testResource);

    proxy.Should().BeAssignableTo<SmsCollectionRule>();
  }

  [Fact]
  public void CallingPropertyGetter_WillRetrieveTheValueFromTheUnderlyingResource_WhenPropertyHasPropertyAttribute() {
    var sut = _proxyGenerator.CreateResourceProxy<SmsCollectionRule>(_testResource);

    sut.Name.Should().Be(_testResource.GetProperty<string>(nameof(SmsCollectionRule.Name)));
  }

  [Fact]
  public void CallingPropertySetter_WillSetTheValueInTheUnderlyingResource_WhenPropertyHasPropertyAttribute() {
    var sut = _proxyGenerator.CreateResourceProxy<SmsCollectionRule>(_testResource);

    var newValue = "NewTestValue";
    sut.Name = newValue;

    _testResource.GetProperty<string>(nameof(SmsCollectionRule.Name)).Should().Be(newValue);
  }

  [Fact]
  public void CallingPropertyGetter_WillRetrieveTheValueFromTheInstance_WhenPropertyHasNoPropertyAttribute() {
    var sut = _proxyGenerator.CreateResourceProxy<SmsCollectionRule>(_testResource);

    sut.NonResourceProperty.Should().Be("Test");
  }

  [Fact]
  public void CallingPropertySetter_WillSetTheValueInTheInstance_WhenPropertyHasNoPropertyAttribute() {
    var sut = _proxyGenerator.CreateResourceProxy<SmsCollectionRule>(_testResource);

    var newValue = "NewTestValue";
    var previousValue = sut.NonResourceProperty;
    sut.NonResourceProperty = newValue;

    _testResource.GetProperty<string>(nameof(SmsCollectionRule.NonResourceProperty)).Should().Be(previousValue);
    sut.NonResourceProperty.Should().Be(newValue);
  }

  [Fact]
  public void CallingPropertyGetter_WillUseTheNameInThePropertyAttribute_WhenGettingResourcePropertyValue() {
    var mockResource = new Mock<IResourceObject>();
    var sut = _proxyGenerator.CreateResourceProxy<SmsCollectionRule>(mockResource.Object);

    var id = sut.Id;

    mockResource.Invocations
                .First(x => x.Method.Name == "GetProperty")
                .Arguments.First().Should().Be("ID");
  }

  [Fact]
  public void CallingPropertySetter_WillUseTheNameInThePropertyAttribute_WhenSettingResourcePropertyValue() {
    var mockResource = new Mock<IResourceObject>();
    var sut = _proxyGenerator.CreateResourceProxy<SmsCollectionRule>(mockResource.Object);

    sut.Id = 10;

    IWqlResourceBase<ISmsCollectionRule> s = new SmsCollectionRule();

    mockResource.Invocations
                .First(x => x.Method.Name == "SetProperty")
                .Arguments.First().Should().Be("ID");
  }
}

[Resource(ClassName = "SMS_CollectionRule")]
public partial class SmsCollectionRule : ISmsCollectionRule {
  public virtual string Name { get; set; }
  public virtual short Id { get; set; }

  public string NonResourceProperty { get; set; } = "Test";

  public SmsCollectionRule() : base(new Mock<IResourceObject>().Object) {
  }

  public T AdaptTo<T>() where T : ISmsCollectionRule {
    throw new NotImplementedException();
  }
}