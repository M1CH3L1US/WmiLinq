using LinqToWql.Infrastructure;
using LinqToWql.Model;
using Microsoft.ConfigurationManagement.ManagementProvider;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace LinqToWql.Data;

internal class ResultObjectAdapter : IResourceObject
{
  public WqlResourceContext Context { get; }

  private IResultObject _wrappedObject;

  public ResultObjectAdapter(WqlResourceContext context, IResultObject objectToWrap) {
    Context = context;
    _wrappedObject = objectToWrap;
  }

  public T GetEmbeddedProperty<T>(string name) where T : IResource {
    return WrapToResource<T>(_wrappedObject.GetSingleItem(name));
  }

  public List<T> GetEmbeddedPropertyList<T>(string name) where T : IResource
  {
    return _wrappedObject.GetArrayItems(name)
      .Select(WrapToResource<T>)
      .ToList();
  }

  public void SetEmbeddedPropertyList<T>(string name, List<T> resourceObjects) where T : IResource {
    var objects = resourceObjects
      .Select(resource => resource.Resource)
      .Select(resource => resource.GetResourceObject<IResultObject>())
      .ToList();

    _wrappedObject.SetArrayItems(name, objects);
  }

  private T WrapToResource<T>(IResultObject resource) {
    return Context.CreateResourceInstance<T>(Wrap(resource));
  }

  private IResourceObject Wrap(IResultObject obj) {
    return new ResultObjectAdapter(Context, obj);
  }

  public T GetResourceObject<T>() {
    return (T) _wrappedObject;
  }

  public void SetEmbeddedProperty<T>(string name, T resource) where T : IResource {
    _wrappedObject.SetSingleItem(name, resource.Resource.GetResourceObject<IResultObject>());
  }

  public WqlResourceProperty<T> GetProperty<T>(string name) {
    var property = GetResourcePropertyInfo<T>();
    var value = property.GetValue(GetResourcePropertyInstance(name));
    return new WqlResourceProperty<T>((T)value);
  }

  public void SetProperty<T>(string name, WqlResourceProperty<T> value) {
    var setValue = value.Value;
    var property = GetResourcePropertyInfo<T>();
    property.SetValue(GetResourcePropertyInstance(name), setValue);
  }


  public IEnumerable<T> GetArrayProperty<T>(string name) {
    var property = GetResourceArrayPropertyInfo<T>();
    var value = property.GetValue(GetResourcePropertyInstance(name)) ?? Enumerable.Empty<T>;

    return ((IEnumerable<object>)value).Cast<T>();
  }

  public void SetArrayProperty<T>(string name, IEnumerable<T> value) {
    var property = GetResourceArrayPropertyInfo<T>();
    property.SetValue(GetResourcePropertyInstance(name), value.ToArray());
  }

  private IQueryPropertyItem GetResourcePropertyInstance(string name) {
    return _wrappedObject[name];
  }

  private PropertyInfo GetResourcePropertyInfo<T>() {
    var accessor = FindBestMatchingPropertyAccessorForNonArrayType<T>();
    return typeof(IQueryPropertyItem).GetProperty(accessor);
  }

  internal static string FindBestMatchingPropertyAccessorForNonArrayType<T>()
  {
    var type = typeof(T);
    var accessorName = type switch
    {
      _ when type == typeof(String) => "String",
      _ when type == typeof(Boolean) => "Boolean",
      _ when type == typeof(DateTime) => "DateTime",
      _ when type == typeof(TimeSpan) => "TimeSpan",
      _ when type == typeof(Int32) => "Integer",
      _ when type == typeof(Int64) => "Long",
      _ => "Object"
    };

    return $"{accessorName}Value";
  }

  private PropertyInfo GetResourceArrayPropertyInfo<T>() {
    var accessor = FindBestMatchingPropertyAccessorForArrayType<T>();
    return typeof(IQueryPropertyItem).GetProperty(accessor);
  }
  private string FindBestMatchingPropertyAccessorForArrayType<T>()
  {
    var type = typeof(T);
    var accessorName = type switch
    {
      _ when type == typeof(String) => "String",
      _ when type == typeof(Boolean) => "Boolean",
      _ when type == typeof(Byte) => "Byte",
      _ when type == typeof(DateTime) => "DateTime",
      _ when type == typeof(Int32) => "Integer",
      _ => "Object",
    };

    return $"{accessorName}ArrayValue";
  }

  public void Update() {
    _wrappedObject.Put();
    _wrappedObject.Get();
  }

  public void Delete() {
    _wrappedObject.Delete();
  }

  public T ExecuteMethod<T>(string name, Dictionary<string, object> parameters) where T : IResource {
    var value = _wrappedObject.ExecuteMethod(name, parameters);
    return WrapToResource<T>(value);
  }
}
