using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

using static LinqToWql.CodeGen.ResourceSourceGenerator;

namespace LinqToWql.CodeGen;

internal class ResourcePropertyType {
  public bool IsEnumerable { get; private set; }
  public bool IsResource { get; private set; }

  public string TypeName { get; private set; }

  public string EnumerableType { get; private set; }

  public WqlPropertyType WqlProperty{ get; private set; }

  public string RequiresNamespace { get; private set; } = "System";

  public ResourcePropertyType(ITypeSymbol typeSymbol) {
    var type = typeSymbol as INamedTypeSymbol;

    if (type is null) {
      throw new NotSupportedException($"Type {typeSymbol} cannot be an array type. Use IEnumerable<T> instead");
    }

    if (type.Name == nameof(IEnumerable<object>)) {
      IsEnumerable = true;
      var enumerableType = type.TypeArguments.First();
      EnumerableType = GetTypeNameAsString(enumerableType);
      IsResource = IsResourceType(enumerableType);
      WqlProperty = ToSupportedPropertyType(enumerableType.ToString());
    }
    else {
      WqlProperty = ToSupportedPropertyType(type.ToString());
      IsResource = IsResourceType(type);
    }

    TypeName = GetTypeNameAsString(type);
  }

  /// <summary>
  /// Creates the field value to access
  /// the resource property of the correct type.
  /// 
  /// IResultObject has different accessors:
  /// StringValue, BooleanValue... some of which also have
  /// an Array accessor StringArrayValue.
  /// </summary>
  /// <returns></returns>
  public string GetResourceFieldName() {
    var field = new StringBuilder();
    
    field.Append(WqlProperty.ToString());

    if (IsEnumerable) {
      field.Append("Array");
    }

    field.Append("Value");

    return field.ToString();
  }

  public string GetWqlResourcePropertyType() {
    return $"WqlResourceProperty<{TypeName}>";
  }

  /// <summary>
  /// There is an issue using Namespaced
  /// types in generic Property types e.g.
  /// public WqlResourceProperty<System.string> {  }
  /// so the namespace is added through a using statement.
  /// </summary>
  /// <param name="type"></param>
  /// <returns></returns>
  private string GetTypeNameAsString(ITypeSymbol type) {
    var propertyTypeString = type.ToString();
    RequiresNamespace = type.ContainingNamespace.ToString();
    return propertyTypeString.Replace($"{RequiresNamespace}.", "");
  }

  private static bool IsResourceType(ITypeSymbol type) {
    var isResource = type
        .GetAttributes()
        .Any(a => a.HasName(EmbeddedResourceAttributeName) || a.HasName(ResourceAttributeName));

    return isResource;
  }

  private static WqlPropertyType ToSupportedPropertyType(string fullName)
  {
    return fullName switch
    {
      "string" or nameof(String) => WqlPropertyType.String,
      "bool" or nameof(Boolean) => WqlPropertyType.Boolean,
      "System.DateTime" or nameof(DateTime) => WqlPropertyType.DateTime,
      "System.TimeSpan" or nameof(TimeSpan) => WqlPropertyType.TimeSpan,
      "int" or nameof(Int32) => WqlPropertyType.Integer,
      "object" or nameof(Object) => WqlPropertyType.Object,
      "long" or nameof(Int64) => WqlPropertyType.Long,
      _ => WqlPropertyType.Object
    };
  }

  internal enum WqlPropertyType {
    String,
    DateTime,
    Integer,
    Long,
    TimeSpan,
    Boolean,
    Object,
  }
}
