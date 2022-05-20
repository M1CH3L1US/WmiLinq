using LinqToWql.Infrastructure;

namespace LinqToWql.Test.Mocks;

public class StubWqlContext : WqlResourceContext {
  public WqlResource<SmsCollection> SmsCollection { get; set; }

  public StubWqlContext(IWqlContextOptions options) : base(options) {
  }
}