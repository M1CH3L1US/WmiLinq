using LinqToWql.Model;

namespace LinqToWql.Test.Mocks.Resources;

public interface ISmsCollectionRule : IWqlResourceBase<ISmsCollectionRule> {
  [Property(Name = "Name")]
  public string Name { get; set; }

  [Property(Name = "ID")]
  public short Id { get; set; }

  public string NonResourceProperty { get; set; }
}