namespace LinqToWql.Model; 

/// <summary>
/// Marks the attributed class as a resource.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class ResourceAttribute : Attribute {
  public string ClassName;
}