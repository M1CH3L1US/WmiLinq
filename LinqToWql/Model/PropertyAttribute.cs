namespace LinqToWql.Model;

[AttributeUsage(AttributeTargets.Field)]
public class PropertyAttribute : Attribute {
  public string Name { get; set; }
  public bool IsList { get; set; } = false;
}