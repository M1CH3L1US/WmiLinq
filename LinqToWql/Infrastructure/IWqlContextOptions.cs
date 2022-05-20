namespace LinqToWql.Infrastructure;

public interface IWqlContextOptions {
  public IWqlConnection WqlConnection { get; }
  public IWqlQueryProcessor WqlQueryProcessor { get; }
}