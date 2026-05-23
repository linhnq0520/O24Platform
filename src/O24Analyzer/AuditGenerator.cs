using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace O24OpenAPI.Generator;

[Generator]
public class AuditGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context
            .SyntaxProvider.CreateSyntaxProvider(
                predicate: (s, _) => s is ClassDeclarationSyntax { AttributeLists.Count: > 0 },
                transform: (ctx, _) => GetSemanticTargetForGeneration(ctx)
            )
            .Where(m => m is not null);

        context.RegisterSourceOutput(classDeclarations, (spc, source) => Execute(source, spc));
    }

    private static ClassDeclarationSyntax GetSemanticTargetForGeneration(
        GeneratorSyntaxContext context
    )
    {
        ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)context.Node;
        foreach (AttributeListSyntax attrList in classDeclaration.AttributeLists)
        {
            foreach (AttributeSyntax attr in attrList.Attributes)
            {
                if (
                    context.SemanticModel.GetSymbolInfo(attr).Symbol is IMethodSymbol outlookSymbol
                    && outlookSymbol.ContainingType.ToDisplayString()
                        == "O24OpenAPI.Core.Attributes.AuditableAttribute"
                )
                {
                    return classDeclaration;
                }
            }
        }
        return null;
    }

    static string GetNamespace(ClassDeclarationSyntax classSyntax)
    {
        var sb = new StringBuilder();
        SyntaxNode? node = classSyntax.Parent;

        while (node != null)
        {
            switch (node)
            {
                case NamespaceDeclarationSyntax ns:
                    if (sb.Length == 0)
                        sb.Insert(0, ns.Name.ToString());
                    else
                        sb.Insert(0, ns.Name + "." + sb);
                    break;

                case FileScopedNamespaceDeclarationSyntax fileNs:
                    if (sb.Length == 0)
                        sb.Insert(0, fileNs.Name.ToString());
                    else
                        sb.Insert(0, fileNs.Name + "." + sb);
                    break;
            }

            node = node.Parent;
        }

        return sb.ToString();
    }

    private static void Execute(
        ClassDeclarationSyntax classDeclaration,
        SourceProductionContext context
    )
    {
        string namespaceName = GetNamespace(classDeclaration);
        string className = classDeclaration.Identifier.Text;

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using O24OpenAPI.Core.Attributes;");
        sb.AppendLine("using O24OpenAPI.Core.Domain;");
        sb.AppendLine($"namespace {namespaceName}");
        sb.AppendLine("{");
        sb.AppendLine($"    public partial class {className} : BaseEntity");
        sb.AppendLine("    {");
        sb.AppendLine($"        public override bool IsAuditable()");
        sb.AppendLine("        {");
        sb.AppendLine("            return true;");
        sb.AppendLine("        }");
        sb.AppendLine($"        public override List<AuditDiff>? GetChanges(BaseEntity oldEntity)");
        sb.AppendLine("        {");
        sb.AppendLine($"            if (oldEntity is not {className} old) return null;");
        sb.AppendLine("            var changes = new List<AuditDiff>();");

        // Duyệt các Property trong Class
        foreach (MemberDeclarationSyntax member in classDeclaration.Members)
        {
            if (member is PropertyDeclarationSyntax prop)
            {
                string propName = prop.Identifier.Text;
                string propType = prop.Type.ToString();

                sb.AppendLine($"            if (this.{propName} != old.{propName})");
                sb.AppendLine("            {");
                sb.AppendLine("                var diff = new AuditDiff { ");
                sb.AppendLine($"                    FieldName = \"{propName}\",");
                sb.AppendLine($"                    OldValue = old.{propName},");
                sb.AppendLine($"                    NewValue = this.{propName}");
                sb.AppendLine("                };");

                if (IsNumericType(propType))
                {
                    // Sử dụng toán tử ?? để thay thế null bằng 0 trước khi thực hiện phép trừ
                    // Ép kiểu sang (decimal?) giúp xử lý chung cho cả kiểu nullable và không nullable
                    sb.AppendLine(
                        $"                diff.Delta = ((decimal?)(this.{propName}) ?? 0m) - ((decimal?)(old.{propName}) ?? 0m);"
                    );
                }

                sb.AppendLine("                changes.Add(diff);");
                sb.AppendLine("            }");
            }
        }

        sb.AppendLine("            return changes;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource($"{className}_Audit.g.cs", sb.ToString());
    }

    private static bool IsNumericType(string type)
    {
        string[] numericTypes =
        {
            "int",
            "decimal",
            "double",
            "float",
            "long",
            "short",
            "int?",
            "decimal?",
            "double?",
        };
        return numericTypes.Contains(type);
    }
}
