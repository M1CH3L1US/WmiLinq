using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LinqToWql.CodeGen;

[Generator]
public class ResourceSourceGenerator : ISourceGenerator {
  public void Initialize(GeneratorInitializationContext context) {
  }

  public const string ResourceAttributeName = "Resource";
  public const string EmbeddedResourceAttributeName = "EmbeddedResource";

  public void Execute(GeneratorExecutionContext context) {
    var compilation = context.Compilation;

    foreach (var syntaxTree in compilation.SyntaxTrees) {
      var semanticModel = compilation.GetSemanticModel(syntaxTree);

      var classesInTree = syntaxTree.GetRoot()
                                    .DescendantNodesAndSelf()
                                    .OfType<ClassDeclarationSyntax>();

      var withResourceAttr = classesInTree.Where(x => x.AttributeLists
                                                       .SelectMany(al => al.Attributes)
                                                       .Any(attributeSyntax =>
                                                       {
                                                         var attributeName = attributeSyntax.Name.ToString();
                                                         return attributeName == ResourceAttributeName || attributeName == EmbeddedResourceAttributeName;
                                                       }));

      var classModels = withResourceAttr.Select(x => semanticModel.GetDeclaredSymbol(x))
                                        .OfType<ITypeSymbol>();


      foreach (var classModel in classModels) {
        var usingStatementsToAdd = new HashSet<string>();
        var resource = AddResourceClass(classModel);
        AddResourceDataImpl(resource, classModel);
        AddPropertyMapping(resource, classModel, usingStatementsToAdd);
        AddClassClosing(resource, classModel);

        var resourceString = PrependNewUsingStatements(resource, usingStatementsToAdd);

        context.AddSource($"{classModel.Name}.g.cs", resourceString);
      }
    }
  }

  private string PrependNewUsingStatements(StringBuilder resource, HashSet<string> usingStatementsToAdd) {
    var usingStatements = usingStatementsToAdd.Select(u => $"using {u};");
    var usingStatementString = string.Join("\n", usingStatements);
    var resourceString = resource.ToString();
    
    return usingStatementString + "\n" + resourceString;
  }

  private StringBuilder AddResourceClass(ITypeSymbol classModel) {
    var sb = new StringBuilder();

    sb.AppendLine("using System;");
    sb.AppendLine("using System.Linq;");
    sb.AppendLine("using Microsoft.ConfigurationManagement.ManagementProvider;");
    sb.AppendLine("using LinqToWql.Model;");
    sb.AppendLine("using LinqToWql.Infrastructure;");
    sb.AppendLine($"namespace {classModel.ContainingNamespace};");
    sb.AppendLine($"public partial class {classModel.Name} : LinqToWql.Model.WqlResourceData<{classModel.Name}> {{");

    return sb;
  }

  private void AddResourceDataImpl(StringBuilder resource, ITypeSymbol classModel) {
    resource.AppendLine(
      $"public {classModel.Name}(WqlResourceContext context, IResultObject resource) : base(context, resource) {{  }}");
    resource.AppendLine($"public {classModel.Name}(WqlResourceContext context) : base(context) {{  }}");
  }

  private void AddPropertyMapping(StringBuilder resource, ITypeSymbol classModel, HashSet<string> namespacesToAppend) {
    var properties = classModel.GetMembers()
                               .OfType<IFieldSymbol>()
                               .Where(p => p.GetAttributes()
                                            .Select(a => a.AttributeClass)
                                            .Any(attribute => attribute?.Name == "PropertyAttribute")
                               );

    foreach (var property in properties) {
      var propertyAttribute = property.GetAttributes()
                                      .SingleOrDefault(attribute =>
                                        attribute.AttributeClass?.Name == "PropertyAttribute"
                                      );

      if (propertyAttribute is null) {
        continue;
      }

      object? GetNamedArgumentFromAttribute(string key) {
        var argument = propertyAttribute.NamedArguments.SingleOrDefault(s => s.Key == key);

        if (argument.Key is null) {
          return null;
        }

        return argument.Value.Value;
      }

      var propertyName = (string) GetNamedArgumentFromAttribute("Name")!;

      var propertyType = new ResourcePropertyType(property.Type);
      var propertyField = new ResourcePropertyBuilder(propertyName, propertyType);

      if (propertyType.IsResource && propertyType.IsEnumerable) {
        AddPropertyEnumerableResource(propertyField, propertyType);
      } else if(propertyType.IsResource) {
        AddPropertyResource(propertyField, propertyType);
      } else if(propertyType.IsEnumerable) {
        AddPropertyEnumerable(propertyField, propertyType);
      } else {
        AddPropertyRegularField(propertyField, propertyType);
      }

      namespacesToAppend.Add(propertyType.RequiresNamespace);
      resource.AppendLine(propertyField.ToString());
    }
  }

  private void AddPropertyRegularField(ResourcePropertyBuilder propertyField, ResourcePropertyType propertyType) {
    propertyField.DefineProperty(propertyType.GetWqlResourcePropertyType());

    propertyField.DefineGetter(appendLine => { 
      appendLine(@$"return ({propertyType.TypeName}) Resource[""{propertyField.PropertyName}""].{propertyType.GetResourceFieldName()};");
    });

    propertyField.DefineSetter(appendLine => {
      appendLine(@$"Resource[""{propertyField.PropertyName}""].{propertyType.GetResourceFieldName()} = value;");
    });
  }

  private void AddPropertyEnumerable(ResourcePropertyBuilder propertyField, ResourcePropertyType propertyType)
  {
    propertyField.DefineProperty($"IEnumerable<{propertyType.EnumerableType}>");

    propertyField.DefineGetter(appendLine => {
      appendLine(@$"var value = Resource[""{propertyField.PropertyName}""].{propertyType.GetResourceFieldName()} ?? Enumerable.Empty<{propertyType.EnumerableType}>();");
      appendLine(@$"return value.Cast<{propertyType.EnumerableType}>();");
    });

    propertyField.DefineSetter(appendLine => {
      appendLine(@$"Resource[""{propertyField.PropertyName}""].{propertyType.GetResourceFieldName()} = value.ToArray();");
    });
  }

  private void AddPropertyResource(ResourcePropertyBuilder propertyField, ResourcePropertyType propertyType) {
    propertyField.DefineComment(AppendWqlResourceWarningComment);

    propertyField.DefineProperty(propertyType.TypeName);

    propertyField.DefineGetter(appendLine => {
      appendLine(@$"var item = Resource.GetSingleItem(""{propertyField.PropertyName}"");");
      appendLine(@$"return Context.CreateResourceInstance<{propertyType.TypeName}>(item);");
    });

    propertyField.DefineSetter(appendLine => {
      appendLine(@$"Resource.SetSingleItem(""{propertyField.PropertyName}"", value.Resource);");
    });
  }
  private void AddPropertyEnumerableResource(ResourcePropertyBuilder propertyField, ResourcePropertyType propertyType) {
    propertyField.DefineComment(AppendWqlResourceWarningComment);

    propertyField.DefineProperty($"List<{propertyType.EnumerableType}>");

    propertyField.DefineGetter(appendLine => {
      appendLine($"return Resource");
      appendLine(@$".GetArrayItems(""{propertyField.PropertyName}"")");
      appendLine($".Select(item => Context.CreateResourceInstance<{propertyType.EnumerableType}>(item))");
      appendLine($".ToList();");
    });

    propertyField.DefineSetter(appendLine => {
      appendLine("var items = value.Select(resource => resource.Resource).ToList();");
      appendLine($@"Resource.SetArrayItems(""{propertyField.PropertyName}"", items);");
    });
  }

  private void AppendWqlResourceWarningComment(AppendLineFn appendLine) {
    appendLine("/// <summary>");
    appendLine("/// This property cannot be used in Linq2Wql queries as it it not possible to query for it using WQL.");
    appendLine("/// Use this property only as a getter / setter for its values.");
    appendLine("/// </summary>");
  }

  private void AddClassClosing(StringBuilder resource, ITypeSymbol classModel) {
    resource.AppendLine("}");
  }
}

