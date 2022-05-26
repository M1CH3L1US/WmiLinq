using LinqToWql.Data;
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
  private ISmsCollectionRule _rules;

  [Property(Name = "SmsIds")]
  private IEnumerable<int> _smsIds;

  public ISmsCollectionRule Rules
  {
    get
    {
      return Resource.GetEmbeddedProperty<ISmsCollectionRule>("Rules");
    }
    set
    {
      Resource.SetEmbeddedProperty<ISmsCollectionRule>("Rules", (WqlResourceData<ISmsCollectionRule>)value.Resource);
    }
  }

  public SmsCollection() : base((IResourceObject) null) {
  }

  public SmsCollection GetSelf() {
    return GetQueryableResource().Single(coll => coll.CollectionId == CollectionId);
  }
}