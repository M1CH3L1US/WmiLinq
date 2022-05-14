namespace LinqToWql.Test.Mocks; 

public class StubContext : WqlResourceContext {
  public StubContext(object connection) : base(connection) {
  }
  
  public WqlResource<SmsCollection> Collections { get; }
}