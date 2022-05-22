using LinqToWql.Infrastructure;
using LinqToWql.Language;
using LinqToWql.Test.Mocks.ResultObject;
using Microsoft.ConfigurationManagement.ManagementProvider;
using Moq;

namespace LinqToWql.Test.Mocks.Stubs;

public class StubWqlQueryProcessor : IWqlQueryProcessor {
  public T ExecuteQuery<T>(string query, QueryResultParseOptions parseOptions) {
    var processor = MakeQueryProcessor();
    return processor.ExecuteQuery<T>(query, parseOptions);
  }

  private WqlQueryProcessorAdapter MakeQueryProcessor() {
    var queryProcessorMock = new Mock<QueryProcessorBase>();

    queryProcessorMock.Setup(p => p.ExecuteQuery(It.IsAny<string>(), null))
                      .Returns(MakeResult);

    var queryProcessor = new WqlQueryProcessorAdapter(queryProcessorMock.Object);

    return queryProcessor;
  }

  private IResultObject MakeResult() {
    var mockData = new Dictionary<string, object> {
      {"Name", "Stub Collection"},
      {"Owner", "Michael"},
      {"SmsIds", "1--11--1"},
      {"CollectionId", "AE.BD.213:130"},
      {"Description", "Test Collection"}
    };

    var mockQueryResult = new StubResultObject(new List<Dictionary<string, object>> {mockData});
    return mockQueryResult;
  }
}