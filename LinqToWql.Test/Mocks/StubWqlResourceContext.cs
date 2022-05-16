using System.Collections;
using LinqToWql.Infrastructure;

namespace LinqToWql.Test.Mocks; 

public class StubWqlResourceContext : IWqlResourceContext {
  public IEnumerable InvokeQuery(string query) {
    return new List<SmsCollection>() {
       new SmsCollection {
        Name = "TestSmsCollection",
        Description = "Test SMS Collection",
      }
    };
  }
}