using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LinqToWql.CodeGen;

[Generator]
public class ResourceSourceGenerator : ISourceGenerator {
  private const string BaseResourceAttribute = "BaseResourceAttribute";
  private const string EmbeddedResourceAttribute = "EmbeddedResourceAttribute";
  private const string ResourceAttribute = "ResourceAttribute";

  private const string DataNamespace = "LinqToWql.Data";
  private const string ModelNamespace = "LinqToWql.Model";
  private const string InfrastructureNamespace = "LinqToWql.Infrastructure";

  private const string ResourceTypeName = "WqlResourceData";
  private const string ResourceBaseTypeName = "IWqlResourceBase";

  public void Initialize(GeneratorInitializationContext context) {
  }

  public void Execute(GeneratorExecutionContext context) {
    var compilation = context.Compilation;

    foreach (var syntaxTree in compilation.SyntaxTrees) {
      var semanticModel = compilation.GetSemanticModel(syntaxTree);
      var declarations = CreateResourceDeclarationForSourceFile(syntaxTree, semanticModel);
      AppendDeclarationsToSourceFile(context, declarations);
    }
  }

  private void AppendDeclarationsToSourceFile(
    GeneratorExecutionContext context,
    Dictionary<string, string> declarations
  ) {
    foreach (var declaration in declarations) {
      context.AddSource($"{declaration.Key}Resource.g.cs", declaration.Value);
    }
  }

  private Dictionary<string, string> CreateResourceDeclarationForSourceFile(
    SyntaxTree syntaxTree,
    SemanticModel semanticModel
  ) {
    var declarations = new Dictionary<string, string>();
    var typeDefinitionsInFile = syntaxTree.GetRoot()
                                          .DescendantNodesAndSelf()
                                          .OfType<MemberDeclarationSyntax>()
                                          .Select(x => semanticModel.GetDeclaredSymbol(x))
                                          .Where(model => model is not null)
                                          .OfType<ITypeSymbol>();

    foreach (var typeDefinition in typeDefinitionsInFile) {
      if (TryCreateResourceDeclaration(typeDefinition, out var declaration)) {
        declarations.Add(declaration.Key, declaration.Value);
      }
    }

    return declarations;
  }

  private bool TryCreateResourceDeclaration(
    ITypeSymbol typeDefinition,
    out KeyValuePair<string, string> declaration
  ) {
    var attributes = typeDefinition.GetAttributes().Select(x => x.AttributeClass!).ToList();

    bool HasAttribute(string type) {
      return attributes.Any(attribute => attribute.Name == type);
    }

    var resourceBuilder = new StringBuilder();
    string resourceDeclaration;

    if (HasAttribute(ResourceAttribute)) {
      resourceDeclaration = AddResourceClass(typeDefinition);
    }
    else if (HasAttribute(EmbeddedResourceAttribute)) {
      resourceDeclaration = AddResourceClass(typeDefinition);
    }
    else if (HasAttribute(BaseResourceAttribute)) {
      resourceDeclaration = AddBaseResourceClass(typeDefinition);
    }
    else {
      declaration = default;
      return false;
    }

    resourceBuilder.AppendLine($"using {DataNamespace};");
    resourceBuilder.AppendLine($"using {ModelNamespace};");
    resourceBuilder.AppendLine($"using {InfrastructureNamespace};");
    resourceBuilder.AppendLine();
    resourceBuilder.AppendLine($"namespace {typeDefinition.ContainingNamespace};");
    resourceBuilder.AppendLine();
    resourceBuilder.AppendLine(resourceDeclaration);

    declaration = new KeyValuePair<string, string>(typeDefinition.Name, resourceBuilder.ToString());
    return true;
  }

  private string AddBaseResourceClass(ITypeSymbol interfaceModel) {
    if (interfaceModel.TypeKind != TypeKind.Interface) {
      throw new Exception($"Base resource {interfaceModel.Name} is not an interface");
    }

    var declaration = new StringBuilder();
    declaration.AppendLine(
      $"public partial interface {interfaceModel.Name} : {ResourceBaseTypeName}<{interfaceModel.Name}> {{");
    declaration.AppendLine("}");
    return declaration.ToString();
  }

  private string AddResourceClass(ITypeSymbol classModel) {
    if (classModel.TypeKind != TypeKind.Class) {
      throw new Exception($"Embedded resource {classModel.Name} is not a class");
    }

    var declaration = new StringBuilder();
    declaration.AppendLine(
      $"public partial class {classModel.Name} : {ResourceTypeName}<{classModel.Name}> {{");
    AddResourceDataImpl(declaration, classModel);
    declaration.AppendLine("}");

    return declaration.ToString();
  }

  private void AddResourceDataImpl(StringBuilder resource, ITypeSymbol classModel) {
    resource.AppendLine(
      $"public {classModel.Name}(IResourceObject resource) : base(resource) {{  }}");
    resource.AppendLine($"public {classModel.Name}(WqlResourceContext context) : base(context) {{  }}");
  }
}