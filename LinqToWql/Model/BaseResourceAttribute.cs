namespace LinqToWql.Model;

/// <summary>
///   Marks the interface as a base resource, meaning
///   that it represents an abstract or base class.
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public class BaseResourceAttribute : ResourceAttribute {
}