using System;
using System.Collections.Generic;
using System.Text;

namespace LinqToWql.CodeGen;

public delegate void AppendLineFn(string str);

internal class ResourcePropertyBuilder {
  private StringBuilder _property;
  public string PropertyName;
  private ResourcePropertyType _propertyType;

  private bool _hasGetter { get; set; }
  private bool _hasDefinition { get; set; }

  public ResourcePropertyBuilder(string propertyName, ResourcePropertyType propertyType) {
    _property = new StringBuilder();
    PropertyName = propertyName;
    _propertyType = propertyType;
  }

  public ResourcePropertyBuilder DefineComment(Action<AppendLineFn> defineComment) {
    if(_hasDefinition) {
      throw new NotSupportedException("Cannot add comment after the definition has been added");
    }

    defineComment(MakeWriteLineAction());

    return this;
  }

  public ResourcePropertyBuilder DefineProperty(string typeName) {
    _property.AppendLine($"public {typeName} {PropertyName} {{");
    _hasDefinition = true;
    return this;
  }

  public ResourcePropertyBuilder DefineGetter(Action<AppendLineFn> defineGetter) {
    _property.AppendLine("get {");
    defineGetter(MakeWriteLineAction());
    _property.AppendLine("}");
    _hasGetter = true;
    return this;
  }

  public ResourcePropertyBuilder DefineSetter(Action<AppendLineFn> defineSetter) {
    _property.AppendLine("set {");
    defineSetter(MakeWriteLineAction());
    _property.AppendLine("}");
    return this;
  }

  private AppendLineFn MakeWriteLineAction() {
    return (string line) => _property.AppendLine(line);
  }

  public override string ToString() {
    if(!_hasDefinition) {
      throw new NotSupportedException("Property must have at least a definition and a getter defined");
    }

    if(!_hasGetter) {
      throw new NotSupportedException("Property must have at least a getter defined");
    }

    // Property closig brace
    _property.AppendLine("}");

    return _property.ToString();
  }
}
