using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Model;

public abstract class WqlResourceData {
  protected ConnectionManagerBase _connectionManager;
  protected IResultObject _resource;

  public WqlResourceData(IResultObject resource, ConnectionManagerBase connectionManager) {
    _resource = resource;
    _connectionManager = connectionManager;
  }

  public IResultObject ExecuteMethod(string command, params Tuple<string, dynamic>[] args) {
    var dictArgs = args.ToDictionary(x => x.Item1, x => x.Item2);
    return _resource.ExecuteMethod(command, dictArgs);
  }

  public Tuple<string, dynamic> Parameter(string name, dynamic value) {
    return new Tuple<string, dynamic>(name, value);
  }

  public void Update() {
    _resource.Put();
    _resource.Get();
  }
}