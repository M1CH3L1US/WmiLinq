using System.Reflection;
using LinqToWql.Language;
using LinqToWql.Model;

namespace LinqToWql.Infrastructure;

public abstract class WqlResourceContext : IDisposable {
  private readonly IWqlContextOptions _options;

  public IWqlConnection Connection => _options.WqlConnection;
  public IWqlQueryProcessor QueryProcessor => _options.WqlQueryProcessor;

  private IQueryProvider QueryProvider { get; }

  public WqlResourceContext(IWqlContextOptions options) {
    _options = options;
    QueryProvider = MakeQueryProvider();
    MapResources();
  }

  public void Dispose() {
    Connection.Dispose();
  }

  public T CreateInstance<T>() where T : WqlResourceData<T> {
    return (T) Activator.CreateInstance(typeof(T), this);
  }

  public WqlResource<T> GetResource<T>() where T : WqlResourceData<T> {
    return new WqlResource<T>(QueryProvider);
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
    var resourceInstance = Activator.CreateInstance(genericType, QueryProvider, null);

    return resourceInstance;
  }

  private Type MakeGenericResourceType(PropertyInfo resourceProperty) {
    var resourceType = resourceProperty.PropertyType.GetGenericArguments().First();
    var genericResourceType = typeof(WqlResource<>).MakeGenericType(resourceType);
    return genericResourceType;
  }
}