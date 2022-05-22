using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Infrastructure;

public class WqlConnectionAdapter : IWqlConnection {
  private readonly ConnectionManagerBase _connection;

  public WqlConnectionAdapter(ConnectionManagerBase connection) {
    _connection = connection;
  }

  public void Dispose() {
    _connection.Dispose();
  }

  public void Connect(string server) {
    _connection.Connect(server);
  }

  public void Connect(string server, string username, string password) {
    _connection.Connect(server, username, password);
  }

  public IResultObject CreateInstance(string className) {
    return _connection.CreateInstance(className);
  }

  public IResultObject ExecuteMethod(string methodClass, string methodName, Dictionary<string, object> parameters) {
    return _connection.ExecuteMethod(methodClass, methodName, parameters);
  }
}