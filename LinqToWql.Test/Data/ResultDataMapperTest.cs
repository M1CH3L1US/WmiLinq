using LinqToWql.Infrastructure;
using LinqToWql.Test.Mocks;
using LinqToWql.Test.Mocks.Resources;

namespace LinqToWql.Test.Data;

public class ResultDataMapperTest {
  private readonly WqlResource<SmsCollection>
    _resource = new ResourceContextBuilder().ConfigureQuery()
                                            .DefineQueryResult(() =>
                                              new SmsCollection {
                                                Name = "Collection",
                                                Description = "Description"
                                              })
                                            .Complete()
                                            .BuildForResource<SmsCollection>();

  [Fact]
  public void Map_MapsIResultObjectToResource_WhenOutputTypeIsResource() {
    var instance = _resource
      .Single(r => r.Name == "Collection");

    instance.Should().BeAssignableTo<SmsCollection>();
  }

  [Fact]
  public void Map_MapsIResultObjectToAnonymousObject_WhenOutputTypeIsAnonymousObject() {
    var instance = _resource
                   .Where(r => r.Description == "Description")
                   .Select(r => new {r.Name, r.CollectionId})
                   .Single();

    instance.Name.Should().NotBeNull();
  }

  [Fact]
  public void Map_MapsIResultObjectSingleProperty_WhenOutputTypeIsProperty() {
    var instance = _resource
                   .Where(r => r.Description == "Description")
                   .Select(r => r.Name)
                   .Single();

    instance.Should().NotBeNull();
  }

  [Fact]
  public void Compile_InvokesQueryWithClosureValueInWhereStatement() {
    var instance = _resource.Single();

    var selfInstance = instance.GetSelf();

    selfInstance.Should().BeAssignableTo<SmsCollection>();
  }
}