using LinqToWql.Model;

namespace LinqToWql.Test.Mocks.Resources;

[EmbeddedResource(ClassName = "SMS_CollectionRuleDirect")]
public partial class SmsCollectionRule {
  [Property(Name = "ResourceClassName")]
  private string _resourceClassName;
}