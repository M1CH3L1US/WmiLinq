using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Infrastructure;

public interface IWqlConnection : IDisposable {
  public void Connect(string server);
  public void Connect(string server, string username, string password);

  public IResultObject CreateInstance(string className);
  public IResultObject ExecuteMethod(string methodClass, string methodName, Dictionary<string, object> parameters);
}