using LinqToWql.Infrastructure;
using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Test.Mocks.Stubs;

public class StubWqlContextOptions : IWqlContextOptions {
  public IWqlConnection WqlConnection { get; set; }
  public IWqlQueryProcessor WqlQueryProcessor { get; set; }

  public StubWqlContextOptions(IResultObject resultObject) {
    WqlConnection = new StubWqlConnection(resultObject);
    WqlQueryProcessor = new StubWqlQueryProcessor(resultObject);
  }
}