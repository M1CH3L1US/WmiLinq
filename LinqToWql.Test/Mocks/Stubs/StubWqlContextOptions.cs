using LinqToWql.Infrastructure;

namespace LinqToWql.Test.Mocks.Stubs;

public class StubWqlContextOptions : IWqlContextOptions {
  public IWqlConnection WqlConnection { get; set; } = new StubWqlConnection();
  public IWqlQueryProcessor WqlQueryProcessor { get; set; } = new StubWqlQueryProcessor();
}