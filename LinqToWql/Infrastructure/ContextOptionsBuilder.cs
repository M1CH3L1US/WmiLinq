using Microsoft.ConfigurationManagement.ManagementProvider;
using Microsoft.ConfigurationManagement.ManagementProvider.WqlQueryEngine;

namespace LinqToWql.Infrastructure;

public class ContextOptionsBuilder {
  private IWqlConnection _connection;

  public ContextOptionsBuilder UseWqlConnection(ConnectionManagerBase connection) {
    _connection = new WqlConnectionAdapter(connection);
    return this;
  }

  public ContextOptionsBuilder UseWqlConnection(string server, string username, string password) {
    var connectionManager = new WqlConnectionManager();
    _connection = new WqlConnectionAdapter(connectionManager);
    _connection.Connect(server, username, password);
    return this;
  }

  public ContextOptionsBuilder UseInMemory() {
    return this;
  }

  public IWqlContextOptions Build() {
    return new WqlContextOptions(_connection);
  }
}