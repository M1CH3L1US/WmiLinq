using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Infrastructure;

public class WqlContextOptions : IWqlContextOptions {
  public IWqlConnection WqlConnection { get; }
  public IWqlQueryProcessor WqlQueryProcessor { get; }

  public WqlContextOptions(ConnectionManagerBase connectionManagerBase) {
    WqlConnection = new WqlConnectionAdapter(connectionManagerBase);
    WqlQueryProcessor = new WqlQueryProcessorAdapter(connectionManagerBase.QueryProcessor);
  }
}