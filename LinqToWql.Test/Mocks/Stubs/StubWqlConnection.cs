using LinqToWql.Infrastructure;
using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Test.Mocks.Stubs;

public class StubWqlConnection : IWqlConnection {
  private readonly IResultObject _resultObject;

  public StubWqlConnection(IResultObject resultObject) {
    _resultObject = resultObject;
  }

  public void Dispose() {
    throw new NotImplementedException();
  }

  public void Connect(string server) {
    throw new NotImplementedException();
  }

  public void Connect(string server, string username, string password) {
    throw new NotImplementedException();
  }

  public IResultObject CreateInstance(string className) {
    return _resultObject;
  }

  public IResultObject ExecuteMethod(string methodClass, string methodName, Dictionary<string, object> parameters) {
    throw new NotImplementedException();
  }

  public IResultObject CreateEmbeddedInstance(string className) {
    throw new NotImplementedException();
  }
}