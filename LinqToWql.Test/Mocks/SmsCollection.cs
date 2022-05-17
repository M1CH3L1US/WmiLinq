using LinqToWql.Model;

namespace LinqToWql.Test.Mocks;

[Resource(Name = "SMS_Collection")]
public partial class SmsCollection {
  [Property(Name = "CollectionId")]
  private string _collectionId;

  [Property(Name = "Description")]
  private string _description;

  [Property(Name = "Name")]
  private string _name;

  [Property(Name = "Owner")]
  private string _owner;

  [Property(Name = "SmsIds", IsList = true)]
  private IEnumerable<int> _smsIds;
}