using LinqToWql.Language;
using LinqToWql.Model;
using Microsoft.ConfigurationManagement.ManagementProvider;

namespace LinqToWql.Data;

public class ResultDataMapper {
  private readonly QueryResultParseOptions _parseOptions;
  private readonly Type _resourceType;
  private readonly IEnumerable<IResultObject> _resultObjects;

  public ResultDataMapper(IEnumerable<IResultObject> resultObjects, QueryResultParseOptions parseOptions) {
    _resultObjects = resultObjects;
    _parseOptions = parseOptions;
    _resourceType = parseOptions.ResourceType;
  }

  public object ApplyTypeMapping() {
    var mappedObjects = _resultObjects.Select(ApplyTypeMappingInternal);
    var resultProcessors = _parseOptions.ResultProcessors;
    object result = mappedObjects;

    foreach (var resultProcessor in resultProcessors) {
      result = resultProcessor((IEnumerable<object>) result!)!;
    }

    return result!;
  }

  private object ApplyTypeMappingInternal(IResultObject resultObject) {
    var resource = MapResultObjectToResource(resultObject);

    if (_parseOptions.ShouldSelectSingleProperty) {
      return MapToSingleResourceProperty(resource);
    }

    if (IsAnonymousType()) {
      return MapToAnonymousType(resource, _parseOptions.QueryResultType);
    }

    return resource;
  }

  private object MapToSingleResourceProperty(WqlResourceData resource) {
    var property = resource.GetType().GetProperty(_parseOptions.SinglePropertyToSelect!)!;
    return property.GetValue(resource);
  }

  private object MapToAnonymousType(WqlResourceData resource, Type anonymousType) {
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

  private WqlResourceData MapResultObjectToResource(IResultObject resultObject) {
    var resourceCtor = _resourceType.GetConstructor(new[] {typeof(IResultObject)})!;
    var resourceInstance = resourceCtor.Invoke(new object[] {resultObject});
    return (WqlResourceData) resourceInstance;
  }
}