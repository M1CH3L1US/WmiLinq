using System.Linq.Expressions;
using LinqToWql.Data;
using LinqToWql.Infrastructure;
using LinqToWql.Language;
using LinqToWql.Model;
using LinqToWql.Test.Mocks.ResultObject;
using LinqToWql.Test.Mocks.Stubs;

namespace LinqToWql.Test.Mocks;

public class ResourceContextBuilder {
  public Dictionary<string, Func<WqlResourceContext, string, string, Dictionary<string, object>, IResourceObject>>
    ExecuteMethodFuncs = new();

  public IResourceObject CreateInstanceResult { get; set; }
  public IResourceObject CreateEmbeddedInstanceResult { get; set; }
  public List<IResourceObject> QueryResult { get; set; } = new();

  public ResourceConnectionBuilder ConfigureConnection() {
    return new ResourceConnectionBuilder(this);
  }

  public ResourceQueryProcessorBuilder ConfigureQuery() {
    return new ResourceQueryProcessorBuilder(this);
  }

  public StubWqlResourceContext Build() {
    var connection = new StubWqlConnection {
      CreateEmbeddedInstanceResult = CreateEmbeddedInstanceResult,
      CreateInstanceResult = CreateInstanceResult,
      ExecuteMethodFuncs = ExecuteMethodFuncs
    };
    var queryProcessor = new StubWqlQueryProcessor();
    var contextOptions = new StubWqlContextOptions {WqlConnection = connection, WqlQueryProcessor = queryProcessor};
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
    if (CreateInstanceResult != null) {
      ((ResourceObject) CreateInstanceResult).Context = context;
    }

    if (CreateEmbeddedInstanceResult != null) {
      ((ResourceObject) CreateEmbeddedInstanceResult).Context = context;
    }

    if (QueryResult != null) {
      QueryResult.ForEach(x => ((ResourceObject) x).Context = context);
    }
  }
}

public class ResourceConnectionBuilder {
  private readonly ResourceContextBuilder _builder;

  public ResourceConnectionBuilder(ResourceContextBuilder builder) {
    _builder = builder;
  }

  public ResourceConnectionBuilder DefineCreateInstance(Expression<Func<IResource>> createInstanceResult) {
    _builder.CreateInstanceResult = MockResourceFactory.CreateResourceObject(createInstanceResult);
    return this;
  }

  public ResourceConnectionBuilder DefineCreateEmbeddedInstance(Expression<Func<IResource>> createInstanceResult) {
    _builder.CreateEmbeddedInstanceResult = MockResourceFactory.CreateResourceObject(createInstanceResult);
    return this;
  }

  public ResourceConnectionBuilder DefineExecuteMethod(string methodName,
    Func<WqlResourceContext, string, string, Dictionary<string, object>, IResourceObject> methodFunc) {
    _builder.ExecuteMethodFuncs.Add(methodName, methodFunc);
    return this;
  }

  public ResourceContextBuilder Complete() {
    return _builder;
  }
}

public class ResourceQueryProcessorBuilder {
  private readonly ResourceContextBuilder _builder;

  public ResourceQueryProcessorBuilder(ResourceContextBuilder builder) {
    _builder = builder;
  }

  public ResourceQueryProcessorBuilder DefineQueryResult(params Expression<Func<IResource>>[] results) {
    _builder.QueryResult = results.Select(x => MockResourceFactory.CreateResourceObject(x)).ToList();
    return this;
  }

  public ResourceContextBuilder Complete() {
    return _builder;
  }
}

public class ResourceBuilder {
}