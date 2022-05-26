using LinqToWql.Infrastructure;
using LinqToWql.Test.Mocks.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToWql.Test.Mocks.Stubs;

public class StubWqlResourceContext : WqlResourceContext
{

  public WqlResource<SmsCollection> SmsCollection { get; set; }

  public StubWqlResourceContext(IWqlContextOptions options) : base(options)
  {
  }
}
