using LinqToWql.Language;

namespace LinqToWql.Test.Mocks; 

public class StubResourceFactory {
  public static WqlResource<T> Create<T>() where T : class {
    return new WqlResource<T>(new WqlQueryProvider(new WqlQueryCompiler()));
  }
}