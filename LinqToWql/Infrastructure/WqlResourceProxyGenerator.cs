using Castle.DynamicProxy;
using LinqToWql.Data;
using LinqToWql.Model;
using System.Reflection;

namespace LinqToWql.Infrastructure;

internal class WqlResourceProxyGenerator {
  /// <summary>
  /// Creates a proxy wrapper for the Resource
  /// in IResultObject.
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="resultObject"></param>
  /// <returns></returns>
  public T CreateResourceInterfaceProxy<T>(IResourceObject objectToWrap) where T : class, IWqlResourceBase<T> {
    var interceptors = GenerateProxyInterceptors(objectToWrap);
    return new ProxyGenerator().CreateInterfaceProxyWithoutTarget<T>(interceptors);
  }

  private IInterceptor[] GenerateProxyInterceptors(IResourceObject objectToWrap) {
    return new[] { new WqlResourceInterceptor(objectToWrap) };
  }

  internal class WqlResourceInterceptor : IInterceptor {
    private IResourceObject _objectToWrap;
    private WqlResourceContext _context;

    public WqlResourceInterceptor(IResourceObject objectToWrap) {
      _objectToWrap = objectToWrap;
      _context = objectToWrap.Context;
    }

    public void Intercept(IInvocation invocation) {
      var methodName = invocation.Method.Name;

      if(methodName == nameof(IWqlResourceBase<object>.AdaptTo)) {

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

    }

    private void InterceptGetterCall(string name, IInvocation invocation) {
      var field = name.Replace("get_", "");
      var propertyType = GetPropertyType(field, invocation);

      var gettetMethod = MakePropertySetterMethod(propertyType!);
      gettetMethod.Invoke(_objectToWrap, new[] { field } );
    }

    private Type GetPropertyType(string fieldName, IInvocation invocation) {
      var property = invocation.TargetType.GetProperty(fieldName);

      if (property is null)
      {
        throw new MissingMemberException($"The field '{fieldName}' is not a property");
      }

      return property.PropertyType;
    }

    private MethodInfo MakePropertyGetterMethod(Type returnType)
    {
      return typeof(IResourceObject)
        .GetMethod(nameof(IResourceObject.GetProperty))
        .MakeGenericMethod(returnType);
    }

    private void InterceptSetterCall(string name, IInvocation invocation) {
      var field = name.Replace("get_", "");
      var propertyType = GetPropertyType(field, invocation);

      var setterMethod = MakePropertySetterMethod(propertyType!);
      var setValue = invocation.Arguments.First();
      setterMethod.Invoke(_objectToWrap, new object[] { field, setValue });
    }

    private MethodInfo MakePropertySetterMethod(Type returnType) {
      return typeof(IResourceObject)
        .GetMethod(nameof(IResourceObject.SetProperty))
        .MakeGenericMethod(returnType);
    }
  }
}
