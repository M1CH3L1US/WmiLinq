using LinqToWql.Infrastructure;

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
}