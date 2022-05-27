namespace LinqToWql.Model;

[AttributeUsage(AttributeTargets.Property)]
public class PropertyAttribute : Attribute {
  public string? Name { get; set; }
}