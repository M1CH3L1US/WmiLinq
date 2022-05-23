using LinqToWql.Model;

namespace LinqToWql.Test.Mocks;

[Resource(ClassName = "SMS_Collection")]
public partial class SmsCollection {
  [Property(Name = "X")]
  private SmsCollectionRule _rules;

  [Property(Name = "CollectionId")]
  private string _collectionId;

  [Property(Name = "Description")]
  private string _description;

  [Property(Name = "Name")]
  private string _name;

  [Property(Name = "Owner")]
  private string _owner;

  [Property(Name = "SmsIds")]
  private IEnumerable<int> _smsIds;

  public SmsCollection GetSelf() {
    return GetQueryableResource()
      .Single(coll => coll.CollectionId == CollectionId);
  }
}