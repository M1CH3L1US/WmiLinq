using LinqToWql.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToWql.Test.Mocks;

public interface ISmsCollectionRule : IWqlResourceBase<ISmsCollectionRule> {
  public string Name { get; set; }
  public Int16 Id { get; set; }
}
