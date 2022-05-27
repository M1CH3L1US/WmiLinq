using LinqToWql.Data;

namespace LinqToWql.Test.Data;

public class EnumerableExtensionsTest {
  [Fact]
  public void Empty_CreatesEmptyEnumerableOfDesiredType() {
    var enumerable = EnumerableExtensions.RuntimeEmpty(typeof(string));

    enumerable.Should().BeOfType<IEnumerable<string>>();
  }
}