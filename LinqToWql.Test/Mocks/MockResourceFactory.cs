using System.Linq.Expressions;
using LinqToWql.Infrastructure;
using LinqToWql.Language;
using LinqToWql.Model;

namespace LinqToWql.Test.Mocks;

public class MockResourceFactory<T> where T : WqlResourceData<T> {
  public static WqlResource<T> CreateWithResultValue(Expression<Func<T>> valueGenerator) {
    var queryResultBuilder = new MockResultObjectBuilder<T>(valueGenerator);

    return CreateWithBuilder(queryResultBuilder);
  }

  public static WqlResource<T> CreateWithBuilder(MockResultObjectBuilder<T> queryResultBuilder) {
    var contextOptions = queryResultBuilder.BuildToContextOptions();
    var ctx = new StubWqlContext(contextOptions);
    var queryRunner = new WqlQueryRunner(ctx);
    var provider = new WqlQueryProvider(queryRunner);

    return new WqlResource<T>(provider);
  }
}