using LinqToWql.Model;

namespace LinqToWql.Test.Mocks;

[Resource(Name = "SMS_Collection")]
public class SmsCollection {
  public WqlResourceProperty<string> Name { get; init; }
  public WqlResourceProperty<string> Description { get; init; }
  public WqlResourceProperty<string> CollectionId { get; init; }
}