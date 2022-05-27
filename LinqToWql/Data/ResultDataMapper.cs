using LinqToWql.Language;

namespace LinqToWql.Data;

public class ResultDataMapper<TResult> {
  private readonly QueryResultParseOptions _parseOptions;
  private readonly Type _resourceType;
  private readonly IEnumerable<IResourceObject> _resultObjects;

  public ResultDataMapper(IEnumerable<IResourceObject> resultObjects, QueryResultParseOptions parseOptions) {
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

  private object ApplyTypeMappingInternal(IResourceObject resultObject) {
    var resource = MapResourceObjectToResource(resultObject);

    if (_parseOptions.ShouldSelectSingleProperty) {
      return MapToSingleResourceProperty(resource);
    }

    if (IsAnonymousType()) {
      return MapToAnonymousType(resource, typeof(TResult));
    }

    return resource;
  }

  private object MapToSingleResourceProperty(object resource) {
    var property = resource.GetType().GetProperty(_parseOptions.SinglePropertyToSelect!)!;
    return property.GetValue(resource);
  }

  private object MapToAnonymousType(object resource, Type anonymousType) {
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
    var genericTypeName = typeof(TResult).Name;
    return genericTypeName.Contains("AnonymousType");
  }

  private object MapResourceObjectToResource(IResourceObject resultObject) {
    return _parseOptions.Context.CreateResourceInstance(_resourceType, resultObject);
  }
}