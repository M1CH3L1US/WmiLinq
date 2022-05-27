using System.Reflection;
using LinqToWql.Data;
using LinqToWql.Language;
using LinqToWql.Model;

namespace LinqToWql.Infrastructure;

public abstract class WqlResourceContext : IDisposable {
  private static readonly WqlResourceProxyGenerator _generator = new();
  private readonly IWqlContextOptions _options;

  private readonly IQueryProvider _queryProvider;

  public IWqlConnection Connection => _options.WqlConnection;
  public IWqlQueryProcessor QueryProcessor => _options.WqlQueryProcessor;

  public WqlResourceContext(IWqlContextOptions options) {
    _options = options;
    _queryProvider = MakeQueryProvider();
    MapResources();
  }

  public void Dispose() {
    Connection.Dispose();
  }

  /// <summary>
  ///   Creates a new wrapped resource of type T
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  public T CreateResourceInstance<T>() where T : WqlResourceData<T> {
    var resource = CreateObject<T>();
    return CreateResourceInstance<T>(resource);
  }

  /// <summary>
  ///   Creates a resource wrapper class of
  ///   type T using the result object provided.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="resource"></param>
  /// <returns></returns>
  public T CreateResourceInstance<T>(IResourceObject resource) {
    return (T) CreateResourceInstance(typeof(T), resource);
  }

  /// <summary>
  ///   Creates a resource wrapper of <see cref="type" />
  ///   using the resource object provided.
  /// </summary>
  /// <param name="type"></param>
  /// <param name="resource"></param>
  /// <returns></returns>
  public IResource CreateResourceInstance(Type type, IResourceObject resource) {
    if (type.IsInterface) {
      return (IResource) _generator.CreateResourceInterfaceProxy(type, resource);
    }

    return (IResource) _generator.CreateResourceProxy(type, resource);
  }

  /// <summary>
  ///   Creates an IResultObject for the
  ///   resource or embedded resource defined
  ///   through the type parameter <see cref="T" />.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  public IResourceObject CreateObject<T>() where T : WqlResourceData<T> {
    var resourceType = typeof(T);
    var resourceAttribute = resourceType.GetCustomAttribute<ResourceAttribute>();
    var embeddedResourceAttribute = resourceType.GetCustomAttribute<EmbeddedResourceAttribute>();

    if (embeddedResourceAttribute is not null) {
      return Connection.CreateEmbeddedInstance(this, embeddedResourceAttribute.ClassName);
    }

    return Connection.CreateInstance(this, resourceAttribute.ClassName);
  }

  public WqlResource<T> GetResource<T>() where T : WqlResourceData<T> {
    return new WqlResource<T>(_queryProvider);
  }

  private IQueryProvider MakeQueryProvider() {
    var queryRunner = new WqlQueryRunner(this);
    return new WqlQueryProvider(queryRunner);
  }

  private void MapResources() {
    var resourceProperties = GetType()
                             .GetProperties()
                             .Where(IsWqlResourceProperty);

    foreach (var resourceProperty in resourceProperties) {
      MakeResourceForProperty(resourceProperty);
    }
  }

  private bool IsWqlResourceProperty(PropertyInfo resourceProperty) {
    // GetGenericTypeDefinition is only supported on generic types
    if (!resourceProperty.PropertyType.IsGenericType) {
      return false;
    }

    var genericType = resourceProperty.PropertyType.GetGenericTypeDefinition();
    return genericType.IsAssignableFrom(typeof(WqlResource<>));
  }

  private void MakeResourceForProperty(PropertyInfo resourceProperty) {
    var resource = MakeResourceInstance(resourceProperty);

    try {
      resourceProperty.SetValue(this, resource);
    }
    catch {
      throw new NotSupportedException($"Property ${resourceProperty.Name} does not have a valid setter.");
    }
  }

  private object MakeResourceInstance(PropertyInfo resourceProperty) {
    var genericType = MakeGenericResourceType(resourceProperty);
    var resourceInstance = Activator.CreateInstance(genericType, _queryProvider, null);

    return resourceInstance;
  }

  private Type MakeGenericResourceType(PropertyInfo resourceProperty) {
    var resourceType = resourceProperty.PropertyType.GetGenericArguments().First();
    var genericResourceType = typeof(WqlResource<>).MakeGenericType(resourceType);
    return genericResourceType;
  }
}