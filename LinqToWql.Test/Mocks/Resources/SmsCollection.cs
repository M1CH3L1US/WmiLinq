using LinqToWql.Model;

namespace LinqToWql.Test.Mocks.Resources;

[Resource(ClassName = "SMS_Collection")]
public partial class SmsCollection {
  [Property(Name = "CollectionId")]
  private string _collectionId;

  [Property(Name = "Description")]
  private string _description;

  [Property(Name = "Name")]
  private string _name;

  [Property(Name = "Owner")]
  private string _owner;

  [Property(Name = "Rules")]
  private SmsCollectionRule _rules;

  [Property(Name = "SmsIds")]
  private IEnumerable<int> _smsIds;

  public SmsCollection() : base(null, null) {
  }

  public SmsCollection GetSelf() {
    return GetQueryableResource().Single(coll => coll.CollectionId == CollectionId);
  }
}