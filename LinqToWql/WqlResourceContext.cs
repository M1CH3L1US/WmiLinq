namespace LinqToWql; 

public abstract class WqlResourceContext {
  protected object Connection;
  
  public WqlResourceContext(object connection) {
    Connection = connection;
  }
}

/*
class MyResource : WqlResourceContext<MyResource> {
  public MyResource(object connection) : base(connection) {
  }
}
*/