using LinqToWql.Data;
using LinqToWql.Infrastructure;
using LinqToWql.Model;

namespace LinqToWql.Test.Mocks.ResultObject;

public class ResourceObject : IResourceObject {
  public ResourceObjectOptions Options;
  public WqlResourceContext Context { get; set; }

  public ResourceObject(ResourceObjectOptions options) {
    Context = options.Context;
    Options = options;
  }

  public T GetProperty<T>(string name) {
    Options.Properties.TryGetValue(name, out var value);
    return (T) value!;
  }

  public void SetProperty<T>(string name, T value) {
    Options.Properties[name] = value!;
  }

  public void Delete() {
    Options.Delete();
  }

  public T ExecuteMethod<T>(string name, Dictionary<string, object> parameters) where T : IResource {
    return Context.CreateResourceInstance<T>(Options.ExecuteMethod(name, parameters));
  }


  public T GetResourceObject<T>() {
    return (T) (object) new StubResultObject(this);
  }

  public void Update() {
    Options.Update();
  }
}

public class ResourceObjectOptions {
  public WqlResourceContext Context { get; set; }
  public Action Delete { get; set; } = () => { };

  public Func<string, Dictionary<string, object>, IResourceObject> ExecuteMethod { get; set; } =
    (string str, Dictionary<string, object> param) => default;

  public Action Update { get; set; } = () => { };

  public Dictionary<string, object> Properties { get; set; } = new();
}