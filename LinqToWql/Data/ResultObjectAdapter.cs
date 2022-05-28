using System.Reflection;
using LinqToWql.Infrastructure;
using LinqToWql.Model;
using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Data;

internal class ResultObjectAdapter : IResourceObject {
  private static readonly Type _propertyItemType = typeof(IQueryPropertyItem);
  private readonly IResultObject _wrappedObject;
  public WqlResourceContext Context { get; }


  public ResultObjectAdapter(WqlResourceContext context, IResultObject objectToWrap) {
    Context = context;
    _wrappedObject = objectToWrap;
  }

  public T GetResourceObject<T>() {
    return (T) _wrappedObject;
  }

  public void Update() {
    _wrappedObject.Put();
    _wrappedObject.Get();
  }

  public void Delete() {
    _wrappedObject.Delete();
  }

  public T ExecuteMethod<T>(string name, Dictionary<string, object> parameters) {
    return (T) _wrappedObject.ExecuteMethod(name, parameters);
  }

  public T GetProperty<T>(string name) {
    var accessor = GetPropertyAccessor<T>(name);
    return accessor.GetValue();
  }

  public void SetProperty<T>(string name, T value) {
    var accessor = GetPropertyAccessor<T>(name);
    accessor.SetValue(value);
  }

  private IResourceObject Wrap(IResultObject obj) {
    return new ResultObjectAdapter(Context, obj);
  }

  private PropertyAccessor<T> GetPropertyAccessor<T>(string propertyName) {
    if (IsResourceEnumerableType<T>(out var resourceType)) {
      return GetEmbeddedListPropertyAccessor<T>(resourceType!, propertyName);
    }

    if (IsResourceType(typeof(T))) {
      return GetEmbeddedPropertyAccessor<T>(propertyName);
    }

    return GetSimplePropertyAccessor<T>(propertyName);
  }

  private PropertyAccessor<T> GetEmbeddedListPropertyAccessor<T>(Type resourceType, string propertyName) {
    return new PropertyAccessor<T>(
      () => (T) _wrappedObject
                .GetArrayItems(propertyName)
                .Select(obj => WrapToResource(resourceType, obj))
                .RuntimeToListCast(resourceType),
      value => {
        var objects = ((IEnumerable<IResource>) value!)
                      .Select(resource => resource.Resource)
                      .Select(resource => resource.GetResourceObject<IResultObject>())
                      .ToList();

        _wrappedObject.SetArrayItems(propertyName, objects);
      }
    );
  }

  private bool IsResourceEnumerableType<T>(out Type? resourceType) {
    resourceType = null;
    var type = typeof(T);

    if (!type.IsGenericType) {
      return false;
    }

    var innerType = type.GetGenericArguments().First();
    var genericEnumerable = typeof(IEnumerable<>).MakeGenericType(innerType);

    if (!genericEnumerable.IsAssignableFrom(type)) {
      return false;
    }

    resourceType = innerType;
    return IsResourceType(innerType);
  }

  private bool IsResourceType(Type type) {
    return typeof(IResource).IsAssignableFrom(type);
  }

  /// <summary>
  ///   Returns a simple property accessor, meaning it
  ///   either accesses IResultObject[propertyName].XValue
  ///   or IResultObject[propertyName].XArrayValue.
  /// </summary>
  /// <param name="propertyName"></param>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  private PropertyAccessor<T> GetSimplePropertyAccessor<T>(string propertyName) {
    var propertyInstance = GetResourcePropertyInstance(propertyName);

    if (IsEnumerableType<T>(out var enumerableType)) {
      var arrayAccessor = FindBestMatchingSimpleArrayProperty(enumerableType!);
      var arrayProperty = GetPropertyItemMemberInfo<PropertyInfo>(arrayAccessor);

      return new PropertyAccessor<T>(
        () => {
          var value = arrayProperty.GetValue(propertyInstance);
          return (T) ((IEnumerable<object>) value).RuntimeCast(enumerableType!);
        },
        value => arrayProperty.SetValue(propertyInstance,
          ((IEnumerable<object>) value!)!.RuntimeToArrayCast(enumerableType!))
      );
    }

    var nonArrayAccessor = FindBestMatchingSimpleProperty(typeof(T));
    var property = GetPropertyItemMemberInfo<PropertyInfo>(nonArrayAccessor);

    return new PropertyAccessor<T>(
      () => (T) property.GetValue(propertyInstance),
      value => property.SetValue(propertyInstance, value)
    );
  }

  private bool IsEnumerableType<T>(out Type? enumerableType) {
    return typeof(T).IsEnumerable(out enumerableType);
  }

  private T GetPropertyItemMemberInfo<T>(string name) where T : MemberInfo {
    return (T) _propertyItemType.GetMember(name)[0];
  }

  private IQueryPropertyItem GetResourcePropertyInstance(string name) {
    return _wrappedObject[name];
  }

  internal static string FindBestMatchingSimpleProperty(Type type) {
    var accessorName = type switch {
      _ when type == typeof(string) => "String",
      _ when type == typeof(bool) => "Boolean",
      _ when type == typeof(DateTime) => "DateTime",
      _ when type == typeof(TimeSpan) => "TimeSpan",
      _ when type == typeof(int) => "Integer",
      _ when type == typeof(long) => "Long",
      _ => "Object"
    };

    return $"{accessorName}Value";
  }

  private string FindBestMatchingSimpleArrayProperty(Type type) {
    var accessorName = type switch {
      _ when type == typeof(string) => "String",
      _ when type == typeof(bool) => "Boolean",
      _ when type == typeof(byte) => "Byte",
      _ when type == typeof(DateTime) => "DateTime",
      _ when type == typeof(int) => "Integer",
      _ => "Object"
    };

    return $"{accessorName}ArrayValue";
  }

  private PropertyAccessor<T> GetEmbeddedPropertyAccessor<T>(string propertyName) {
    return new PropertyAccessor<T>(
      () => {
        var item = _wrappedObject.GetSingleItem(propertyName);
        return WrapToResource<T>(item);
      },
      value => {
        var wrappedResource = ((IResource) value!).Resource;
        _wrappedObject.SetSingleItem(propertyName, wrappedResource.GetResourceObject<IResultObject>());
      });
  }

  private T WrapToResource<T>(IResultObject resource) {
    return Context.CreateResourceInstance<T>(Wrap(resource));
  }

  private IResource WrapToResource(Type resourceType, IResultObject resource) {
    var genericCreateResourceMethod = Context.GetType()
                                             .GetMethod(
                                               nameof(WqlResourceContext.CreateResourceInstance),
                                               new[] {typeof(IResourceObject)}
                                             )!
                                             .MakeGenericMethod(resourceType);

    return (IResource) genericCreateResourceMethod.Invoke(Context, new object[] {Wrap(resource)});
  }

  internal readonly ref struct PropertyAccessor<T> {
    private readonly Func<T> _valueGetter;
    private readonly Action<T> _valueSetter;

    public PropertyAccessor(Func<T> valueGetter, Action<T> valueSetter) {
      _valueGetter = valueGetter;
      _valueSetter = valueSetter;
    }

    public T GetValue() {
      return _valueGetter();
    }

    public void SetValue(T value) {
      _valueSetter(value);
    }
  }
}