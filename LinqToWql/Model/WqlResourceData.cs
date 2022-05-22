using System.Reflection;
using LinqToWql.Infrastructure;
using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Model;

public abstract class WqlResourceData<T> {
  protected readonly WqlResourceContext Context;
  protected readonly IResultObject Resource;

  /// <summary>
  ///   Creates a new WqlResourceData wrapper object
  ///   for <see cref="resource" />.
  /// </summary>
  /// <param name="context"></param>
  /// <param name="resource"></param>
  public WqlResourceData(WqlResourceContext context, IResultObject resource) {
    Context = context;
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
    var instanceName = GetResourceClassName();
    Resource = context.Connection.CreateInstance(instanceName);
  }

  /// <summary>
  ///   Executes a method on the resource object.
  ///   Use <see cref="Context" />.ExecuteMethod instead to execute
  ///   methods in a global context.
  /// </summary>
  /// <param name="command"></param>
  /// <param name="args"></param>
  /// <returns></returns>
  protected IResultObject ExecuteMethod(string command, params Tuple<string, dynamic>[] args) {
    var dictArgs = args.ToDictionary(x => x.Item1, x => (object) x.Item2);
    return Resource.ExecuteMethod(command, dictArgs);
  }

  protected Tuple<string, dynamic> Parameter(string name, dynamic value) {
    return new Tuple<string, dynamic>(name, value);
  }

  /// <summary>
  ///   Updates the wrapped resource on the remote server.
  /// </summary>
  public void Update() {
    Resource.Put();
    Resource.Get();
  }

  /// <summary>
  ///   Returns the name of the resouce class
  ///   defined through the <see cref="ResourceAttribute" /> attribute.
  /// </summary>
  /// <returns></returns>
  private string GetResourceClassName() {
    var resourceAttribute = GetType().GetCustomAttribute<ResourceAttribute>();
    return resourceAttribute.Name;
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
  ///   Deletes the resource.
  /// </summary>
  public void Delete() {
    Resource.Delete();
  }
}