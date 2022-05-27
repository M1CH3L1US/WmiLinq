using System.Reflection;
using LinqToWql.Data;
using LinqToWql.Infrastructure;

namespace LinqToWql.Model;

public abstract class WqlResourceData<T> : IResource where T : WqlResourceData<T> {
  protected readonly WqlResourceContext Context;
  public IResourceObject Resource { get; }

  /// <summary>
  ///   Creates a new WqlResourceData wrapper object
  ///   for <see cref="resource" />.
  /// </summary>
  /// <param name="resource"></param>
  public WqlResourceData(IResourceObject resource) {
    Context = resource.Context;
    Resource = resource;

    ValidateProperties();
  }

  /// <summary>
  ///   Creates a new instance of this resource type
  ///   using the resource context and wraps that object
  ///   using this instance.
  /// </summary>
  /// <param name="context"></param>
  public WqlResourceData(WqlResourceContext context) : this(context.CreateObject<T>()) {
  }

  private void ValidateProperties() {
    var propertiesWithPropertyAttribute = GetType()
                                          .GetProperties()
                                          .Where(
                                            property => property.GetCustomAttribute<PropertyAttribute>() is not null
                                          );

    foreach (var property in propertiesWithPropertyAttribute) {
      var isPropertyVirtual = property.GetGetMethod().IsVirtual;

      if (!isPropertyVirtual) {
        throw new Exception($"Property ${property.Name} in {GetType().Name} is not virtual." +
                            "Either remove the PropertyAttribute on the property or make it virtual");
      }
    }
  }

  /// <summary>
  ///   Executes a method on the resource object.
  ///   Use <see cref="Context" />.ExecuteMethod instead to execute
  ///   methods in a global context.
  /// </summary>
  /// <param name="command"></param>
  /// <param name="args"></param>
  /// <returns></returns>
  protected TResult ExecuteMethod<TResult>(string command, params Tuple<string, dynamic>[] args)
    where TResult : IResource {
    var dictArgs = args.ToDictionary(x => x.Item1, x => (object) x.Item2);
    return Resource.ExecuteMethod<TResult>(command, dictArgs);
  }

  protected Tuple<string, dynamic> Parameter(string name, dynamic value) {
    return new Tuple<string, dynamic>(name, value);
  }

  /// <summary>
  ///   Updates the wrapped resource on the remote server.
  /// </summary>
  public void Update() {
    Resource.Update();
  }

  /// <summary>
  ///   Returns a queryable WqlResource of the
  ///   current instance type.
  /// </summary>
  /// <returns></returns>
  protected WqlResource<T> GetQueryableResource() {
    return Context.GetResource<T>();
  }

  /// <summary>
  ///   Gets a resource from the context of type T.
  /// </summary>
  /// <typeparam name="TResource"></typeparam>
  /// <returns></returns>
  protected WqlResource<TResource> GetResource<TResource>() where TResource : WqlResourceData<TResource> {
    return Context.GetResource<TResource>();
  }

  /// <summary>
  ///   Deletes the resource.
  /// </summary>
  public void Delete() {
    Resource.Delete();
  }
}