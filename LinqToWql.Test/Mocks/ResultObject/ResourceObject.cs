using LinqToWql.Data;
using LinqToWql.Infrastructure;
using LinqToWql.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToWql.Test.Mocks.ResultObject;

public class ResourceObject : IResourceObject {
  public WqlResourceContext Context { get; set; }

  public ResourceObjectOptions Options;
  public ResourceObject(ResourceObjectOptions options) {
    Context = Options.Context;
    Options = options;
  }

  public void Delete()
  {
    Options.Delete();
  }

  public T ExecuteMethod<T>(string name, Dictionary<string, object> parameters) where T : IResource
  {
    return Context.CreateResourceInstance<T>(Options.ExecuteMethod(name, parameters));
  }

  public IEnumerable<T> GetArrayProperty<T>(string name)
  {
    return Options.ListProperties[name].Cast<T>();
  }

  public T GetEmbeddedProperty<T>(string name) where T : IResource
  {
    return (T)Options.EmbeddedProperties[name];
  }

  public List<T> GetEmbeddedPropertyList<T>(string name) where T : IResource
  {
    return Options.EmbeddedListProperties[name].Cast<T>().ToList();
  }

  public WqlResourceProperty<T> GetProperty<T>(string name)
  {
    return new WqlResourceProperty<T>((T)Options.Properties[name]);
  }

  public T GetResourceObject<T>() {
    return (T)(object)this;
  }

  public void SetArrayProperty<T>(string name, IEnumerable<T> value)
  {
    Options.ListProperties[name] = value.Cast<object>().ToList();
  }

  public void SetEmbeddedProperty<T>(string name, T resource) where T : IResource
  {
    Options.EmbeddedProperties[name] = resource;
  }

  public void SetEmbeddedPropertyList<T>(string name, List<T> resourceObjects) where T : IResource
  {
    Options.EmbeddedListProperties[name] = resourceObjects.Cast<IResource>().ToList();
  }

  public void SetProperty<T>(string name, WqlResourceProperty<T> value)
  {
    Options.Properties[name] = value.Value;
  }

  public void Update()
  {
    Options.Update();
  }
}

public class ResourceObjectOptions {
  public WqlResourceContext Context { get; set; }
  public Action Delete { get; set; } = () => { };
  public Func<string, Dictionary<string, object>, IResourceObject> ExecuteMethod { get; set; } = (string str, Dictionary<string, object> param) => default;
  public Action Update { get; set; } = () => { };

  public Dictionary<string, IResource> EmbeddedProperties { get; set; } = new();
  public Dictionary<string, List<IResource>> EmbeddedListProperties { get; set; } = new();
  public Dictionary<string, object> Properties { get; set; } = new();
  public Dictionary<string, List<object>> ListProperties { get; set; } = new();
}