using System.Collections;
using LinqToWql.Infrastructure;

namespace LinqToWql.Test.Mocks;

public class StubWqlResourceContext : WqlResourceContext {
  public IWqlConnection Connection { get; }
  public IWqlQueryProcessor QueryProcessor { get; }

  public StubWqlResourceContext(IWqlContextOptions options) : base(options) {
  }

  public IEnumerable InvokeQuery(string query) {
    return new List<SmsCollection>();
  }
}