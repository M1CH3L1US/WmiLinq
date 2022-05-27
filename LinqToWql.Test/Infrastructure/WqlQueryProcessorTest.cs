using LinqToWql.Infrastructure;
using LinqToWql.Test.Mocks;
using LinqToWql.Test.Mocks.Resources;

namespace LinqToWql.Test.Infrastructure;

public class WqlQueryProcessorTest {
  private readonly WqlResource<SmsCollection> _resource = new ResourceContextBuilder()
                                                          .ConfigureQuery()
                                                          .DefineQueryResult(() => new SmsCollection {Name = "Foo"})
                                                          .Complete()
                                                          .BuildForResource<SmsCollection>();

  [Fact]
  public void Map_MapsIResultObjectToResource_WhenOutputTypeIsResource() {
    var instances = _resource.Where(r => r.Description == "Foo");
    var count = 0;

    foreach (var instance in instances) {
      count++;
      instance.Should().BeAssignableTo<SmsCollection>();
    }

    count.Should().Be(1);
  }

  [Fact]
  public void ApplyQueryTransformation_AppliesCustomQueryTransformationFromStatements_WhenQueryHasSuchStatements() {
    var instances = _resource.Where(r => r.Description == "Foo");

    instances.Count().Should().Be(1);
  }
}