using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LinqToWql.CodeGen;

[Generator]
public class ResourceSourceGenerator : ISourceGenerator {
  public void Initialize(GeneratorInitializationContext context) {
  }

  public void Execute(GeneratorExecutionContext context) {
    var compilation = context.Compilation;
    var resourceAttributeName = "Resource";

    foreach (var syntaxTree in compilation.SyntaxTrees) {
      var semanticModel = compilation.GetSemanticModel(syntaxTree);

      var classesInTree = syntaxTree.GetRoot()
                                    .DescendantNodesAndSelf()
                                    .OfType<ClassDeclarationSyntax>();

      var withResourceAttr = classesInTree.Where(x => x.AttributeLists
                                                       .SelectMany(al => al.Attributes)
                                                       .Any(attributeSyntax =>
                                                         attributeSyntax.Name.ToString() ==
                                                         resourceAttributeName));

      var classModels = withResourceAttr.Select(x => semanticModel.GetDeclaredSymbol(x))
                                        .OfType<ITypeSymbol>();


      foreach (var classModel in classModels) {
        var resource = AddResourceClass(classModel);
        AddResourceDataImpl(resource, classModel);
        AddPropertyMapping(resource, classModel);
        AddClassClosing(resource, classModel);
        context.AddSource($"{classModel.Name}.g.cs", resource.ToString());
      }
    }
  }

  private StringBuilder AddResourceClass(ITypeSymbol classModel) {
    var sb = new StringBuilder();

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

  private void AddPropertyMapping(StringBuilder resource, ITypeSymbol classModel) {
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
      var isArray = (bool) (GetNamedArgumentFromAttribute("IsList") ?? false);
      var propertyType = property.Type;

      var propertyGenericType = "";
      var fullTypeName = propertyType.ToString();
      var regex = new Regex(".*\\<(.*)\\>.*");
      var match = regex.Match(fullTypeName);
      var typeGenericArgumentCollection = match.Groups?[1];

      if (typeGenericArgumentCollection is not null) {
        propertyGenericType = typeGenericArgumentCollection.Value;
      }

      resource.AppendLine($"public WqlResourceProperty<{propertyType}> {propertyName} {{");

      if (isArray) {
        resource.AppendLine(
          $@"get => new WqlResourceProperty<{propertyType}>(Resource[""{propertyName}""].ObjectArrayValue.Cast<{propertyGenericType}>());");
        resource.AppendLine(
          $@"set => Resource[""{propertyName}""].ObjectArrayValue = value.Value.Cast<object>().ToArray();");
      }
      else {
        resource.AppendLine($@"get => ({propertyType}) Resource[""{propertyName}""].ObjectValue;");
        resource.AppendLine($@"set => Resource[""{propertyName}""].ObjectValue = value;");
      }

      resource.AppendLine("}");
    }
  }

  private void AddClassClosing(StringBuilder resource, ITypeSymbol classModel) {
    resource.AppendLine("}");
  }
}