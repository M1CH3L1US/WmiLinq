using LinqToWql.Data;
using LinqToWql.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToWql.Test.Mocks.Stubs;

public class StubWqlConnection : IWqlConnection {
  public IResourceObject CreateInstanceResult { get; set; }
  public IResourceObject CreateEmbeddedInstanceResult { get; set; }

  public Dictionary<string, Func<WqlResourceContext, string, string, Dictionary<string, object>, IResourceObject>> ExecuteMethodFuncs = new();


  public void Connect(string server)
  {
  }

  public void Connect(string server, string username, string password)
  {
  }

  public IResourceObject CreateEmbeddedInstance(WqlResourceContext context, string className)
  {
    return CreateEmbeddedInstanceResult;
  }

  public IResourceObject CreateInstance(WqlResourceContext context, string className)
  {
    return CreateInstanceResult;
  }

  public void Dispose()
  {
    throw new NotImplementedException();
  }

  public IResourceObject ExecuteMethod(WqlResourceContext context, string methodClass, string methodName, Dictionary<string, object> parameters)
  {
    return ExecuteMethodFuncs[methodName](context, methodClass, methodName, parameters);
  }
}
