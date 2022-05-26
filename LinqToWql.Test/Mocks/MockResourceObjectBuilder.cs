using System.Linq.Expressions;
using System.Reflection;
using LinqToWql.Data;
using LinqToWql.Infrastructure;
using LinqToWql.Model;
using LinqToWql.Test.Mocks.ResultObject;
using LinqToWql.Test.Mocks.Stubs;
using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Test.Mocks;

public class MockResourceObjectBuilder<T> where T : IResource {
  private readonly List<Expression<Func<T>>> _innerItems = new();
  private readonly List<MockResourceObjectBuilder<T>> _builders = new();
  private ResourceObjectOptions _options = new();

  public MockResourceObjectBuilder() {
  }

  public MockResourceObjectBuilder(Expression<Func<T>> generator) {
    _innerItems.Add(generator);
  }
  
  public MockResourceObjectBuilder<T> WithDeleteMethod(Action deleteMethod) {
    _options.Delete = deleteMethod;
    return this;
  }

  public MockResourceObjectBuilder<T> WithExecuteMethod(Func<string, Dictionary<string, object>, IResourceObject> executeMethod)
  {
    _options.ExecuteMethod = executeMethod;
    return this;
  }

  public MockResourceObjectBuilder<T> WithUpdateMethod(Action updateMethod)
  {
    _options.Update = updateMethod;
    return this;
  }

  public MockResourceObjectBuilder<T> WithMultipleResults(params Expression<Func<T>>[] resultBuilder) {
    _innerItems.AddRange(resultBuilder);
    return this;
  }

  public MockResourceObjectBuilder<T> WithMultipleResults(params MockResourceObjectBuilder<T>[] resultBuilder) {
    _builders.AddRange(resultBuilder);
    return this;
  }

  public MockResourceObjectBuilder<T> WithContext(WqlResourceContext context) {
    _options.Context = context;
    return this;
  }

  public IResourceObject Build() {
    return new ResourceObject(_options);
  }

  public List<IResourceObject> BuildAsQueryResult() {
    if(_builders.Count != 0) {
      return _builders.Select(x => x.Build()).ToList();
    }

    return _innerItems
      .Select(i => new MockResourceFactory().CreateResourceObject<T>(i))
      .Cast<IResourceObject>()
      .ToList();
  }
}