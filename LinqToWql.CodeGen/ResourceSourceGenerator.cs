using System.Text;
using LinqToWql.Data;
using LinqToWql.Infrastructure;
using LinqToWql.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LinqToWql.CodeGen;

[Generator]
public class ResourceSourceGenerator : ISourceGenerator {
  private readonly Type _baseResourceAttribute = typeof(BaseResourceAttribute);
  private readonly Type _embeddedResourceAttribute = typeof(EmbeddedResourceAttribute);
  private readonly Type _resourceAttribute = typeof(ResourceAttribute);

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

    bool HasAttribute(Type type) {
      return attributes.Any(attribute => HasSameNameAndNamespace(attribute, type));
    }

    var resourceBuilder = new StringBuilder();
    string resourceDeclaration;

    if (HasAttribute(_resourceAttribute)) {
      resourceDeclaration = AddResourceClass(typeDefinition);
    }
    else if (HasAttribute(_embeddedResourceAttribute)) {
      resourceDeclaration = AddResourceClass(typeDefinition);
    }
    else if (HasAttribute(_baseResourceAttribute)) {
      resourceDeclaration = AddBaseResourceClass(typeDefinition);
    }
    else {
      declaration = default;
      return false;
    }

    resourceBuilder.AppendLine($"using {typeof(IResource).Namespace};");
    resourceBuilder.AppendLine($"using {typeof(WqlResourceContext).Namespace};");
    resourceBuilder.AppendLine($"using {typeof(IResourceObject).Namespace};");
    resourceBuilder.AppendLine($"namespace {typeDefinition.ContainingNamespace};");
    resourceBuilder.AppendLine();
    resourceBuilder.AppendLine(resourceDeclaration);

    declaration = new KeyValuePair<string, string>(typeDefinition.Name, resourceBuilder.ToString());
    return true;
  }

  private bool HasSameNameAndNamespace(ISymbol typeSymbol, Type type) {
    return typeSymbol.Name == type.Name
           && typeSymbol.ContainingNamespace.ToString() == type.Namespace;
  }

  private string AddBaseResourceClass(ITypeSymbol interfaceModel) {
    if (interfaceModel.TypeKind != TypeKind.Interface) {
      throw new Exception($"Base resource {interfaceModel.Name} is not an interface");
    }

    var declaration = new StringBuilder();
    declaration.AppendLine(
      $"public partial interface {interfaceModel.Name} : {GetNameOfGenericType(typeof(IWqlResourceBase<>))}<{interfaceModel.Name}> {{");
    declaration.AppendLine("}");
    return declaration.ToString();
  }

  private string AddResourceClass(ITypeSymbol classModel) {
    if (classModel.TypeKind != TypeKind.Class) {
      throw new Exception($"Embedded resource {classModel.Name} is not a class");
    }

    var declaration = new StringBuilder();
    declaration.AppendLine(
      $"public partial class {classModel.Name} : {GetNameOfGenericType(typeof(WqlResourceData<>))}<{classModel.Name}> {{");
    AddResourceDataImpl(declaration, classModel);
    declaration.AppendLine("}");

    return declaration.ToString();
  }

  private void AddResourceDataImpl(StringBuilder resource, ITypeSymbol classModel) {
    resource.AppendLine(
      $"public {classModel.Name}(IResourceObject resource) : base(resource) {{  }}");
    resource.AppendLine($"public {classModel.Name}(WqlResourceContext context) : base(context) {{  }}");
  }

  private string GetNameOfGenericType(Type type) {
    var name = type.Name;

    if (!type.IsGenericType) {
      return name;
    }

    return name.Substring(0, name.IndexOf('`'));
  }
}