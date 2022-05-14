using LinqToWql.Model;

namespace LinqToWql.Test.Mocks; 

[Resource(Name = "SMS_Collection")]
public class SmsCollection {
  public WqlResourceProperty<string> Name { get; set; }
  public WqlResourceProperty<string> Description { get; set; }
  public WqlResourceProperty<string> CollectionID { get; set; }
}