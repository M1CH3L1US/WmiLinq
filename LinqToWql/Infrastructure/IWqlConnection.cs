namespace LinqToWql.Infrastructure;

public interface IWqlConnection : IDisposable {
  public void Connect(string server);
  public void Connect(string server, string username, string password);
}