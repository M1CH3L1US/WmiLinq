using System.Reflection;
using Castle.DynamicProxy;
using LinqToWql.Data;
using LinqToWql.Model;

namespace LinqToWql.Infrastructure;

internal class WqlResourceProxyGenerator {
  private static readonly IProxyGenerator _generator = new ProxyGenerator();

  /// <summary>
  ///   Creates a proxy instance of type
  ///   <see cref="T" />, wrapping the <see cref="resourceObject" />
  ///   provided. Member accesses of that object will be proxied
  ///   to the underlying resource object.
  /// </summary>
  /// <param name="resourceObject"></param>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  public T CreateResourceProxy<T>(IResourceObject resourceObject) where T : WqlResourceData<T> {
    return (T) CreateResourceProxy(typeof(T), resourceObject);
  }

  public object CreateResourceProxy(Type resourceType, IResourceObject resourceObject) {
    var resourceArguments = new object[] {resourceObject};
    var interceptors = GenerateProxyInterceptors(resourceType, resourceObject);
    return _generator.CreateClassProxy(resourceType, resourceArguments, interceptors);
  }

  /// <summary>
  ///   Creates a proxy instance of type
  ///   <see cref="T" />, wrapping the <see cref="resourceObject" />
  ///   provided. Member accesses of that object will be proxied
  ///   to the underlying resource object.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="resourceObject"></param>
  /// <returns></returns>
  public T CreateResourceInterfaceProxy<T>(IResourceObject resourceObject) where T : class, IWqlResourceBase<T> {
    return (T) CreateResourceInterfaceProxy(typeof(T), resourceObject);
  }

  public object CreateResourceInterfaceProxy(Type interfaceType, IResourceObject resourceObject) {
    var interceptors = GenerateProxyInterceptors(interfaceType, resourceObject);
    return _generator.CreateInterfaceProxyWithoutTarget(interfaceType, interceptors);
  }

  private IInterceptor[] GenerateProxyInterceptors(Type typeToProxy, IResourceObject resourceObject) {
    return new[] {new WqlResourceInterceptor(resourceObject, typeToProxy)};
  }

  internal class WqlResourceInterceptor : IInterceptor {
    private static readonly MethodInfo _resourceGetPropertyMethod =
      GetResourceMethod(nameof(IResourceObject.GetProperty));

    private static readonly MethodInfo _resourceSetPropertyMethod =
      GetResourceMethod(nameof(IResourceObject.SetProperty));

    private static readonly Type _genericIWqlResourceBaseType = typeof(IWqlResourceBase<>).GetGenericTypeDefinition();

    private readonly WqlResourceContext _context;
    private readonly IResourceObject _resourceObject;
    private readonly Type _targetType;

    public WqlResourceInterceptor(IResourceObject resourceObject, Type targetType) {
      _resourceObject = resourceObject;
      _context = resourceObject.Context;
      _targetType = targetType;
    }

    public void Intercept(IInvocation invocation) {
      var methodName = invocation.Method.Name;

      if (methodName == nameof(IWqlResourceBase<object>.AdaptTo)) {
        TryAdaptToResource(invocation);
        return;
      }

      if (methodName.StartsWith("get_")) {
        InterceptGetterCall(methodName, invocation);
        return;
      }

      if (methodName.StartsWith("set_")) {
        InterceptSetterCall(methodName, invocation);
        return;
      }

      throw new MissingMethodException("Method calls are not supported through a proxied resource.");
    }

    private void TryAdaptToResource(IInvocation invocation) {
      var resourceType = invocation.GenericArguments.First();
      invocation.ReturnValue = _context.CreateResourceInstance(resourceType, _resourceObject);
    }

    private void InterceptGetterCall(string name, IInvocation invocation) {
      var field = name.Replace("get_", "");
      var propertyName = GetPropertyNameInResource(field);
      var propertyType = GetPropertyType(field, invocation);

      var gettetMethod = _resourceGetPropertyMethod.MakeGenericMethod(propertyType);
      invocation.ReturnValue = gettetMethod.Invoke(_resourceObject, new[] {propertyName});
    }

    private void InterceptSetterCall(string name, IInvocation invocation) {
      var field = name.Replace("set_", "");
      var propertyName = GetPropertyNameInResource(field);
      var propertyType = GetPropertyType(field, invocation);

      var setterMethod = _resourceSetPropertyMethod.MakeGenericMethod(propertyType);
      var setValue = invocation.Arguments.First();
      setterMethod.Invoke(_resourceObject, new[] {propertyName, setValue});
    }

    private string GetPropertyNameInResource(string field) {
      var propertyAttribute = GetPropertyAttributeFromType(_targetType, field);

      if (propertyAttribute?.Name is not null) {
        return propertyAttribute.Name;
      }

      var inheritedBaseInterface = _targetType
                                   .GetInterfaces()
                                   .SingleOrDefault(TypeInheritsFromWqlResourceBase);

      if (inheritedBaseInterface is null) {
        return field;
      }

      var inheritedPropertyAttribute = GetPropertyAttributeFromType(inheritedBaseInterface, field);
      return inheritedPropertyAttribute?.Name ?? field;
    }

    private bool TypeInheritsFromWqlResourceBase(Type type) {
      return type.GetInterfaces()
                 .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == _genericIWqlResourceBaseType);
    }

    private PropertyAttribute? GetPropertyAttributeFromType(Type type, string propertyName) {
      return type.GetProperty(propertyName)!
                 .GetCustomAttribute<PropertyAttribute>();
    }

    private Type GetPropertyType(string fieldName, IInvocation invocation) {
      var property = _targetType.GetProperty(fieldName);

      if (property is null) {
        throw new MissingMemberException($"The field '{fieldName}' is not a property");
      }

      return property.PropertyType;
    }

    private static MethodInfo GetResourceMethod(string methodName) {
      return typeof(IResourceObject).GetMethod(methodName)!;
    }
  }
}