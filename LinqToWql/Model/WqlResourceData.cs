using System.Reflection;
using LinqToWql.Data;
using LinqToWql.Infrastructure;
using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Model;

public abstract class WqlResourceData<T> where T : WqlResourceData<T> {
  protected readonly WqlResourceContext Context;
  public IResourceObject Resource { get; }

  /// <summary>
  ///   Creates a new WqlResourceData wrapper object
  ///   for <see cref="resource" />.
  /// </summary>
  /// <param name="context"></param>
  /// <param name="resource"></param>
  public WqlResourceData(IResourceObject resource) {
    Context = resource.Context;
    Resource = resource;
  }

  /// <summary>
  ///   Creates a new instance of this resource type
  ///   using the resource context and wraps that object
  ///   using this instance.
  /// </summary>
  /// <param name="context"></param>
  public WqlResourceData(WqlResourceContext context) {
    Context = context;
    Resource = context.CreateObject<T>();
  }

  /// <summary>
  ///   Executes a method on the resource object.
  ///   Use <see cref="Context" />.ExecuteMethod instead to execute
  ///   methods in a global context.
  /// </summary>
  /// <param name="command"></param>
  /// <param name="args"></param>
  /// <returns></returns>
  protected T ExecuteMethod<T>(string command, params Tuple<string, dynamic>[] args) where T : IResource {
    var dictArgs = args.ToDictionary(x => x.Item1, x => (object) x.Item2);
    return Resource.ExecuteMethod<T>(command, dictArgs);
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