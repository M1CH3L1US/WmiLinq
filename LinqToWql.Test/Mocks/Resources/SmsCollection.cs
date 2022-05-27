using LinqToWql.Data;
using LinqToWql.Model;

namespace LinqToWql.Test.Mocks.Resources;

[Resource(ClassName = "SMS_Collection")]
public partial class SmsCollection {
  [Property(Name = "Description")]
  public virtual string Description { get; set; }

  [Property(Name = "Name")]
  public virtual string Name { get; set; }

  [Property(Name = "Owner")]
  public virtual string Owner { get; set; }

  [Property(Name = "Rules")]
  public virtual ISmsCollectionRule Rules { get; set; }

  [Property(Name = "SmsIds")]
  public virtual IEnumerable<int> SmsIds { get; set; }

  [Property(Name = "CollectionId")]
  public virtual string CollectionId { get; set; }

  public SmsCollection() : base((IResourceObject) null) {
  }

  public SmsCollection GetSelf() {
    return GetQueryableResource().Single(coll => coll.CollectionId == CollectionId);
  }
}