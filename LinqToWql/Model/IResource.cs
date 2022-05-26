using LinqToWql.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToWql.Model;

public interface IResource
{
  public IResourceObject Resource { get; }
}
