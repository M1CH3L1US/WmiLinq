using LinqToWql.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToWql.Test.Data;

public class EnumerableExtensionsTest {
  [Fact]
  public void Empty_CreatesEmptyEnumerableOfDesiredType() {
    var enumerable = EnumerableExtensions.Empty(typeof(string));

    enumerable.Should().BeOfType<IEnumerable<string>>();
  }
}
