using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToWql.CodeGen;

internal static class AttributeDataExtensions {
  public static T GetArgumentValue<T>(this AttributeData attribute, string argumentName) {
    var argument = attribute.NamedArguments.Single(arg => arg.Key == argumentName);
    return (T) argument.Value.Value!;
  }

  public static bool HasName(this AttributeData attribute, string name) {
    if(!name.EndsWith("Attribute")) {
      name = $"{name}Attribute";
    }

    return attribute.AttributeClass?.Name == name;
  }
}
