using LinqToWql.Language;
using LinqToWql.Model;
using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Data;

public class ResultDataMapper {
  private readonly QueryResultParseOptions _parseOptions;
  private readonly Type _resourceType;
  private readonly IResultObject _resultObject;

  public ResultDataMapper(IResultObject resultObject, QueryResultParseOptions parseOptions) {
    _resultObject = resultObject;
    _parseOptions = parseOptions;
    _resourceType = parseOptions.ResourceType;
  }

  public object ApplyTypeMapping() {
    var resource = MapToResource();

    if (_parseOptions.ShouldSelectSingleProperty) {
      return SelectSingleResourceProperty(resource);
    }

    if (IsAnonymousType()) {
      return MakeAnonymousType(resource, _parseOptions.QueryResultType);
    }

    return resource;
  }

  private object SelectSingleResourceProperty(WqlResourceData resource) {
    var property = resource.GetType().GetProperty(_parseOptions.SinglePropertyToSelect!)!;
    return property.GetValue(resource);
  }

  private object MakeAnonymousType(WqlResourceData resource, Type anonymousType) {
    var resourceType = resource.GetType();
    var properties = anonymousType.GetProperties();
    // Anonymous types have a single constructor
    // whose arguments is all it's properties in the same
    // order as they appear in the object.
    var anonymousTypeCtorArgs = new List<object>();

    foreach (var property in properties) {
      var resourcePropertyInfo = resourceType.GetProperty(property.Name)!;
      var resourcePropertyValue = resourcePropertyInfo.GetValue(resource);
      anonymousTypeCtorArgs.Add(resourcePropertyValue);
    }

    return Activator.CreateInstance(anonymousType, anonymousTypeCtorArgs.ToArray());
  }

  private bool IsAnonymousType() {
    var genericTypeName = _parseOptions.QueryResultType.Name;
    return genericTypeName.Contains("AnonymousType");
  }

  private WqlResourceData MapToResource() {
    var resourceCtor = _resourceType.GetConstructor(new[] {typeof(IResultObject)})!;
    var resourceInstance = resourceCtor.Invoke(new object[] {_resultObject});
    return (WqlResourceData) resourceInstance;
  }
}