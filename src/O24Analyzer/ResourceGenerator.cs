using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace O24OpenAPI.Generator;

[Generator]
public sealed class ResourceGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor MissingLocalizedRule = new(
        id: "RC001",
        title: "Missing Localized attribute",
        messageFormat: "Const string field '{0}' in class '{1}' must have [Localized] attribute",
        category: "Localization",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classSymbols = context
            .SyntaxProvider.CreateSyntaxProvider(
                predicate: static (node, _) =>
                    node is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0,
                transform: static (ctx, _) =>
                    ctx.SemanticModel.GetDeclaredSymbol(ctx.Node) as INamedTypeSymbol
            )
            .Where(static symbol => symbol is not null);

        var combinedData = classSymbols.Combine(context.CompilationProvider);

        context.RegisterSourceOutput(
            combinedData,
            (spc, source) =>
            {
                var classSymbol = source.Left!;
                var compilation = source.Right;

                var resourceAttrSymbol = compilation.GetTypeByMetadataName(
                    "O24OpenAPI.Core.Attributes.StringResourceAttribute"
                );
                var localizedAttrSymbol = compilation.GetTypeByMetadataName(
                    "O24OpenAPI.Core.Attributes.LocalizedAttribute"
                );

                if (resourceAttrSymbol is null || localizedAttrSymbol is null)
                    return;

                bool hasResourceAttribute = classSymbol
                    .GetAttributes()
                    .Any(a =>
                        SymbolEqualityComparer.Default.Equals(a.AttributeClass, resourceAttrSymbol)
                    );

                if (!hasResourceAttribute)
                    return;

                ProcessClassRecursive(spc, classSymbol, localizedAttrSymbol);
            }
        );
    }

    private void ProcessClassRecursive(
        SourceProductionContext spc,
        INamedTypeSymbol classSymbol,
        INamedTypeSymbol localizedAttrSymbol
    )
    {
        foreach (var member in classSymbol.GetMembers())
        {
            if (member is IFieldSymbol field)
            {
                if (field.IsConst && field.Type.SpecialType == SpecialType.System_String)
                {
                    ValidateField(spc, field, localizedAttrSymbol);
                }
            }
            else if (
                member is INamedTypeSymbol nestedClass
                && nestedClass.TypeKind == TypeKind.Class
            )
            {
                ProcessClassRecursive(spc, nestedClass, localizedAttrSymbol);
            }
        }
    }

    private void ValidateField(
        SourceProductionContext spc,
        IFieldSymbol field,
        INamedTypeSymbol localizedAttrSymbol
    )
    {
        var localizedAttr = field
            .GetAttributes()
            .FirstOrDefault(a =>
                SymbolEqualityComparer.Default.Equals(a.AttributeClass, localizedAttrSymbol)
            );

        if (localizedAttr is null)
        {
            spc.ReportDiagnostic(
                Diagnostic.Create(
                    MissingLocalizedRule,
                    field.Locations.Length > 0 ? field.Locations[0] : Location.None,
                    field.Name,
                    field.ContainingType.Name
                )
            );
            return;
        }
    }
}
