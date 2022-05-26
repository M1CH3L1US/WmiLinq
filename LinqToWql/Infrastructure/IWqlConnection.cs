using LinqToWql.Data;

namespace LinqToWql.Infrastructure;

public interface IWqlConnection : IDisposable {
  public void Connect(string server);
  public void Connect(string server, string username, string password);

  public IResourceObject CreateInstance(WqlResourceContext context, string className);
  public IResourceObject CreateEmbeddedInstance(WqlResourceContext context, string className);
  public IResourceObject ExecuteMethod(WqlResourceContext context, string methodClass, string methodName, Dictionary<string, object> parameters);
}