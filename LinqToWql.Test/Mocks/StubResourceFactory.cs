using LinqToWql.Infrastructure;
using LinqToWql.Language;
using LinqToWql.Test.Mocks.Stubs;

namespace LinqToWql.Test.Mocks;

public static class StubResourceFactory {
  public static WqlResource<T> Create<T>() where T : class {
    return new WqlResource<T>(
      new WqlQueryProvider(
        new WqlQueryRunner(
          new StubWqlResourceContext(
            new StubWqlContextOptions()
          )
        )
      )
    );
  }
}