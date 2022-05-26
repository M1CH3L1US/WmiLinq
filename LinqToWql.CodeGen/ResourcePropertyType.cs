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
    }
    else {
      IsResource = IsResourceType(type);
    }

    TypeName = GetTypeNameAsString(type);
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
    var isResourceImplementation = type
        .GetAttributes()
        .Any(a => a.HasName(EmbeddedResourceAttributeName) || a.HasName(ResourceAttributeName));

    var isResouceBase = type.Interfaces.Any(iface => iface.Name == "IWqlResourceBase");

    return isResourceImplementation || isResouceBase;
  }
}
