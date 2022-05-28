using LinqToWql.Infrastructure;

namespace LinqToWql.Data;

/// <summary>
///   A generic wrapper for an object.
/// </summary>
public interface IResourceObject {
  public WqlResourceContext Context { get; }

  public T? GetProperty<T>(string name);
  public void SetProperty<T>(string name, T value);

  /// <summary>
  ///   Returns the wrapped resource object.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  public T GetResourceObject<T>();

  public void Update();
  public void Delete();
  public T ExecuteMethod<T>(string name, Dictionary<string, object> parameters);
}