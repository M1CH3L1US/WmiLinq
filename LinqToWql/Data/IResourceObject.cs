using LinqToWql.Infrastructure;
using LinqToWql.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToWql.Data;

/// <summary>
/// A wrapper for IResultObject
/// </summary>
public interface IResourceObject {
  public WqlResourceContext Context { get; }

  public T GetEmbeddedProperty<T>(string name) where T : IResource;
  public void SetEmbeddedProperty<T>(string name, T resource) where T : IResource;

  public List<T> GetEmbeddedPropertyList<T>(string name) where T : IResource;
  public void SetEmbeddedPropertyList<T>(string name, List<T> resourceObjects) where T : IResource;

  public WqlResourceProperty<T> GetProperty<T>(string name);
  public void SetProperty<T>(string name, WqlResourceProperty<T> value);

  public IEnumerable<T> GetArrayProperty<T>(string name);

  public void SetArrayProperty<T>(string name, IEnumerable<T> value);

  /// <summary>
  /// Returns the wrapped resource object.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  public T GetResourceObject<T>();

  public void Update();

  public void Delete();

  public T ExecuteMethod<T>(string name, Dictionary<string, object> parameters) where T : IResource;
}
