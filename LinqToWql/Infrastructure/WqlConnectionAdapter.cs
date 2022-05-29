using LinqToWql.Data;
using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Infrastructure;

public class WqlConnectionAdapter : IWqlConnection {
  private readonly ConnectionManagerBase _connection;

  public IWqlQueryProcessor QueryProcessor { get; }

  public WqlConnectionAdapter(ConnectionManagerBase connection) {
    _connection = connection;
    QueryProcessor = new WqlQueryProcessorAdapter(connection.QueryProcessor);
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

  public IResourceObject CreateInstance(WqlResourceContext context, string className) {
    var instance = _connection.CreateInstance(className);
    return new ResultObjectAdapter(context, instance);
  }

  public IResourceObject CreateEmbeddedInstance(WqlResourceContext context, string className) {
    var instance = _connection.CreateEmbeddedObjectInstance(className);
    return new ResultObjectAdapter(context, instance);
  }

  public IResourceObject ExecuteMethod(WqlResourceContext context, string methodClass, string methodName,
    Dictionary<string, object> parameters) {
    var result = _connection.ExecuteMethod(methodClass, methodName, parameters);
    return new ResultObjectAdapter(context, result);
  }
}