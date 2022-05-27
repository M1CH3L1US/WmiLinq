using LinqToWql.Data;

namespace LinqToWql.Test.Data;

public class EnumerableExtensionsTest {
  [Fact]
  public void Empty_CreatesEmptyEnumerableOfDesiredType() {
    var enumerable = EnumerableExtensions.RuntimeEmpty(typeof(string));
    //This actually returns an EmptyPartition<string> so we need to use
    // IsAssignableTo instead of IsOfType
    enumerable.Should().BeAssignableTo<IEnumerable<string>>();
  }
}