using LinqToWql.Data;
using LinqToWql.Infrastructure;
using LinqToWql.Language;
using Microsoft.ConfigurationManagement.ManagementProvider;
using Moq;

namespace LinqToWql.Test.Mocks.Stubs;

public class StubWqlQueryProcessor : IWqlQueryProcessor {
  public List<IResourceObject> QueryResult { get; set; }

  public readonly List<string> Queries = new();

  /// <summary>
  ///   The last query that was run by the processor
  /// </summary>
  public string? LastQuery => Queries.SingleOrDefault();

  public IEnumerable<IResourceObject> ExecuteQuery(string query, QueryResultParseOptions parseOptions)
  {
    Queries.Add(query);
    return QueryResult;
  }
}