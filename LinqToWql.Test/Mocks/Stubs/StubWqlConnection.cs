using LinqToWql.Infrastructure;
using LinqToWql.Test.Mocks.ResultObject;
using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Test.Mocks.Stubs;

public class StubWqlConnection : IWqlConnection {
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
    var obj = new Dictionary<string, object>();
    return new StubResultObject(new List<Dictionary<string, object>> {obj});
  }

  public IResultObject ExecuteMethod(string methodClass, string methodName, Dictionary<string, object> parameters) {
    throw new NotImplementedException();
  }
}