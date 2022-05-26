﻿using LinqToWql.Infrastructure;
using LinqToWql.Test.Mocks;
using LinqToWql.Test.Mocks.Resources;
using LinqToWql.Test.Mocks.Stubs;

namespace LinqToWql.Test.Data;

public class ResultDataMapperTest {
  private WqlResource<SmsCollection> _resource = new ResourceContextBuilder().BuildForResource<SmsCollection>();

  [Fact]
  public void Map_MapsIResultObjectToResource_WhenOutputTypeIsResource() {
    var instance = _resource
      .Single(r => r.Name == "Collection");

    instance.Should().BeOfType<SmsCollection>();
  }

  [Fact]
  public void Map_MapsIResultObjectToAnonymousObject_WhenOutputTypeIsAnonymousObject() {
    var instance = _resource
                   .Where(r => r.Description == "Foo")
                   .Select(r => new {r.Name, r.CollectionId})
                   .Single();

    instance.Name.Value.Should().NotBeNull();
  }

  [Fact]
  public void Map_MapsIResultObjectSingleProperty_WhenOutputTypeIsProperty() {
    var instance = _resource
                   .Where(r => r.Description == "Foo")
                   .Select(r => r.Name)
                   .Single();

    instance.Value.Should().NotBeNull();
  }

  [Fact]
  public void Compile_InvokesQueryWithClosureValueInWhereStatement() {
    var instance = _resource.Single();

    var selfInstance = instance.GetSelf();

    selfInstance.Should().BeOfType<SmsCollection>();
  }
}