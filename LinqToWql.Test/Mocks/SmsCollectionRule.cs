using LinqToWql.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToWql.Test.Mocks;

[EmbeddedResource(ClassName = "SMS_CollectionRuleDirect")]
public partial class SmsCollectionRule {
  [Property(Name = "ResourceClassName")]
  private string _resourceClassName;
}
