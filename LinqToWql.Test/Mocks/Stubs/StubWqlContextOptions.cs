using LinqToWql.Infrastructure;
using LinqToWql.Language;

namespace LinqToWql.Test.Mocks.Stubs;

public class StubWqlContextOptions : IWqlContextOptions {
  public IWqlConnection Connection { get; set; }
  public IWqlQueryRunner QueryRunner { get; set; }
}