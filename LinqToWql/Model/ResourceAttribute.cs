namespace LinqToWql.Model; 

[AttributeUsage(AttributeTargets.Class)]
public class ResourceAttribute : Attribute {
  public string Name;
}