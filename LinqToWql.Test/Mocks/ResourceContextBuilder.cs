using LinqToWql.Infrastructure;
using LinqToWql.Language;
using LinqToWql.Test.Mocks.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using LinqToWql.Test.Mocks.Stubs;
using LinqToWql.Test.Mocks.ResultObject;
using LinqToWql.Data;
using System.Linq.Expressions;
using System.Reflection;
using LinqToWql.Model;

namespace LinqToWql.Test.Mocks;

public class ResourceContextBuilder {
  public IResourceObject CreateInstanceResult { get; set; }
  public IResourceObject CreateEmbeddedInstanceResult { get; set; }
  public List<IResourceObject> QueryResult { get; set; } = new();

  public Dictionary<string, Func<WqlResourceContext, string, string, Dictionary<string, object>, IResourceObject>> ExecuteMethodFuncs = new();

  public ResourceConnectionBuilder ConfigureConnection() {
    return new ResourceConnectionBuilder(this);
  }

  public ResourceQueryProcessorBuilder ConfigureQuery() {
    return new ResourceQueryProcessorBuilder(this);
  }

  public StubWqlResourceContext Build() {
    var connection = new StubWqlConnection()
    {
      CreateEmbeddedInstanceResult = CreateEmbeddedInstanceResult,
      CreateInstanceResult = CreateInstanceResult,
      ExecuteMethodFuncs = ExecuteMethodFuncs,
    };
    var queryProcessor = new StubWqlQueryProcessor();
    var contextOptions = new StubWqlContextOptions() { WqlConnection = connection, WqlQueryProcessor = queryProcessor };
    var context = new StubWqlResourceContext(contextOptions);
    
    SetContext(context);

    queryProcessor.QueryResult = QueryResult;

    return context;
  }

  public WqlResource<T> BuildForResource<T>() {
    var context = Build();
    return new WqlResource<T>(new WqlQueryProvider(new WqlQueryRunner(context)));
  }

  private void SetContext(WqlResourceContext context) {
    ((ResourceObject)CreateInstanceResult).Context = context;
    ((ResourceObject)CreateEmbeddedInstanceResult).Context = context;

    QueryResult.ForEach(x => ((ResourceObject)x).Context = context); 
  }
}

public class ResourceConnectionBuilder  {
  private ResourceContextBuilder _builder;

  public ResourceConnectionBuilder(ResourceContextBuilder builder) {
    _builder = builder;
  }

  public ResourceConnectionBuilder DefineCreateInstance(Expression<Func<IResource>> createInstanceResult) {
    _builder.CreateInstanceResult = new MockResourceFactory().CreateResourceObject(createInstanceResult);
    return this;
  }

  public ResourceConnectionBuilder DefineCreateEmbeddedInstance(Expression<Func<IResource>> createInstanceResult) {
    _builder.CreateEmbeddedInstanceResult = new MockResourceFactory().CreateResourceObject(createInstanceResult);
    return this;
  }

  public ResourceConnectionBuilder DefineExecuteMethod(string methodName, Func<WqlResourceContext, string, string, Dictionary<string, object>, IResourceObject> methodFunc) {
    _builder.ExecuteMethodFuncs.Add(methodName, methodFunc);
    return this;
  }

  public ResourceContextBuilder Complete() {
    return _builder;
  }
}

public class ResourceQueryProcessorBuilder {
  private ResourceContextBuilder _builder;

  public ResourceQueryProcessorBuilder(ResourceContextBuilder builder) {
    _builder = builder;
  }

  public ResourceQueryProcessorBuilder DefineQueryResult(params Expression<Func<IResource>>[] results) {
    _builder.QueryResult = results.Select(x => new MockResourceFactory().CreateResourceObject(x)).ToList();
    return this;
  } 

  public ResourceContextBuilder Complete() {
    return _builder;
  }
}

public class ResourceBuilder {

}