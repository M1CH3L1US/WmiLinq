using LinqToWql.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToWql.Test.Mocks.Stubs;

public class StubWqlContextOptions : IWqlContextOptions
{
  public IWqlConnection WqlConnection { get; set; }
  public IWqlQueryProcessor WqlQueryProcessor { get; set; }
}
