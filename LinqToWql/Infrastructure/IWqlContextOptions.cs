using LinqToWql.Language;

namespace LinqToWql.Infrastructure;

public interface IWqlContextOptions {
  public IWqlConnection Connection { get; }
  public IWqlQueryRunner QueryRunner { get; }
}