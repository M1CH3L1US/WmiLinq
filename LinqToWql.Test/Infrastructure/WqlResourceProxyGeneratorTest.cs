using LinqToWql.Data;
using LinqToWql.Infrastructure;
using LinqToWql.Model;
using LinqToWql.Test.Mocks;
using LinqToWql.Test.Mocks.Stubs;
using Microsoft.ConfigurationManagement.ManagementProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToWql.Test.Infrastructure;

public class WqlResourceProxyGeneratorTest {
  private IResourceObject testResouce = new MockResourceObjectBuilder<ISmsCollectionRule>(() => new SmsCollectionRuleImpl
  {
    Name = "Foo",
    Id = 1
  }).Build();

  [Fact]
  public void CreateResourceInterfaceProxy_CreatesAnObjectOfTheInterfaceType_WhenTypeIsInterface() {
    var sut = new WqlResourceProxyGenerator();

    var proxy = sut.CreateResourceInterfaceProxy<ISmsCollectionRule>(testResouce);

    proxy.Should().BeOfType<ISmsCollectionRule>();
  }

  [Fact]
  public void CreateResourceInterfaceProxy_InterfaceFieldsAreProxyiedToTheInterface_WhenFieldIsGetter() {
    var sut = new WqlResourceProxyGenerator();

    var proxy = sut.CreateResourceInterfaceProxy<ISmsCollectionRule>(testResouce);

    proxy.Name.Should().Be("Foo");
  }

  [Fact]
  public void CreateResourceInterfaceProxy_InterfaceFieldsAreProxyiedToTheInterface_WhenFieldIsSetter()
  {
    var sut = new WqlResourceProxyGenerator();

    var proxy = sut.CreateResourceInterfaceProxy<ISmsCollectionRule>(testResouce);

    proxy.Name = "Bar";

    proxy.Name.Should().Be("Bar");
  }
}

[Resource(ClassName = "SMS_CollectionRule")]
public partial class SmsCollectionRuleImpl : ISmsCollectionRule { 
  public string Name { get; set; }

  public Int16 Id { get; set; }

  public SmsCollectionRuleImpl() : base((WqlResourceContext)null) {
  }

  public T AdaptTo<T>() where T : ISmsCollectionRule
  {
    throw new NotImplementedException();
  }

}
