using LinqToWql.Infrastructure;
using LinqToWql.Language;
using Microsoft.ConfigurationManagement.ManagementProvider;
using Moq;

namespace LinqToWql.Test.Mocks.Stubs;

public class StubWqlQueryProcessor : IWqlQueryProcessor {
  private readonly IResultObject _queryResult;

  public readonly List<string> Queries = new();

  /// <summary>
  ///   The last query that was run by the processor
  /// </summary>
  public string? LastQuery => Queries.SingleOrDefault();

  public StubWqlQueryProcessor(IResultObject queryResult) {
    _queryResult = queryResult;
  }

  public T ExecuteQuery<T>(string query, QueryResultParseOptions parseOptions) {
    Queries.Add(query);
    var processor = MakeQueryProcessor();
    return processor.ExecuteQuery<T>(query, parseOptions);
  }

  private WqlQueryProcessorAdapter MakeQueryProcessor() {
    var queryProcessorMock = new Mock<QueryProcessorBase>();

    queryProcessorMock.Setup(p => p.ExecuteQuery(It.IsAny<string>(), null))
                      .Returns(_queryResult);

    var queryProcessor = new WqlQueryProcessorAdapter(queryProcessorMock.Object);

    return queryProcessor;
  }
}