using System.Collections;

namespace LinqToWql.Infrastructure; 

public abstract class WqlResourceContext : IWqlResourceContext {
  protected object Connection;
  
  public WqlResourceContext(object connection) {
    Connection = connection;
  }

  public IEnumerable InvokeQuery(string query) {
    throw new NotImplementedException();
  }
}

/*
class MyResource : WqlResourceContext<MyResource> {
  public MyResource(object connection) : base(connection) {
  }
}
*/