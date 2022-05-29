using LinqToWql.Language;

namespace LinqToWql.Infrastructure;

public class WqlContextOptions : IWqlContextOptions {
  public IWqlConnection Connection { get; }
  public IWqlQueryRunner QueryRunner { get; }

  public WqlContextOptions(IWqlConnection connection) {
    Connection = connection;
    QueryRunner = new WqlQueryRunner(connection.QueryProcessor);
  }
}