using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace O24OpenAPI.Generators;

[Generator]
public class GrpcMediatorGenerator : IIncrementalGenerator
{
    private const string AttributeName = "GrpcClientAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. Tìm các service kế thừa BaseGrpcClientService (Logic cũ)
        IncrementalValuesProvider<ClassDeclarationSyntax?> serviceDeclarations = context
            .SyntaxProvider.CreateSyntaxProvider(
                predicate: (s, _) => s is ClassDeclarationSyntax,
                transform: (ctx, _) => GetServiceTarget(ctx)
            )
            .Where(m => m is not null);

        // 2. Tìm các class Command có gắn Attribute [GrpcClient] (Logic mới)
        IncrementalValuesProvider<DirectCommandMetadata?> directCommandDeclarations = context
            .SyntaxProvider.CreateSyntaxProvider(
                predicate: (s, _) =>
                    s is ClassDeclarationSyntax cds && cds.AttributeLists.Count > 0,
                transform: (ctx, _) => GetDirectCommandTarget(ctx)
            )
            .Where(m => m is not null);

        IncrementalValueProvider<(
            (Compilation Left, ImmutableArray<ClassDeclarationSyntax?> Right) Left,
            ImmutableArray<DirectCommandMetadata?> Right
        )> combined = context
            .CompilationProvider.Combine(serviceDeclarations.Collect())
            .Combine(directCommandDeclarations.Collect());

        context.RegisterSourceOutput(
            combined,
            (spc, source) => Execute(source.Left.Left, source.Left.Right, source.Right, spc)
        );
    }

    private static ClassDeclarationSyntax? GetServiceTarget(GeneratorSyntaxContext context)
    {
        ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)context.Node;
        ISymbol? symbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);

        if (symbol is INamedTypeSymbol classSymbol)
        {
            INamedTypeSymbol? baseType = classSymbol.BaseType;
            while (baseType != null)
            {
                if (baseType.Name == "BaseGrpcClientService")
                    return classDeclaration;
                baseType = baseType.BaseType;
            }
        }
        return null;
    }

    private static DirectCommandMetadata? GetDirectCommandTarget(GeneratorSyntaxContext context)
    {
        ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax)context.Node;
        INamedTypeSymbol? symbol =
            context.SemanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

        if (symbol == null)
            return null;

        AttributeData? attr = symbol
            .GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == AttributeName);
        if (attr == null || attr.ConstructorArguments.Length < 2)
            return null;

        INamedTypeSymbol? grpcClientType = attr.ConstructorArguments[0].Value as INamedTypeSymbol;
        var methodName = attr.ConstructorArguments[1].Value as string;

        if (grpcClientType == null || string.IsNullOrEmpty(methodName))
            return null;

        IMethodSymbol methodSymbol = grpcClientType
            .GetMembers(methodName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault();
        if (methodSymbol == null || methodSymbol.Parameters.Length == 0)
            return null;

        ITypeSymbol requestType = methodSymbol.Parameters[0].Type;

        return new DirectCommandMetadata
        {
            CommandSymbol = symbol,
            GrpcClientFullType = grpcClientType.ToDisplayString(),
            MethodName = methodName,
            RequestFullType = requestType.ToDisplayString(),
            ResponseType =
                symbol
                    .Interfaces.FirstOrDefault(i => i.Name == "ICommand" && i.IsGenericType)
                    ?.TypeArguments.FirstOrDefault()
                    ?.ToDisplayString() ?? "Unit",
            Properties = symbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => !p.IsStatic && p.DeclaredAccessibility == Accessibility.Public)
                .Select(p => p.Name)
                .ToList(),
        };
    }

    private void Execute(
        Compilation compilation,
        ImmutableArray<ClassDeclarationSyntax?> serviceClasses,
        ImmutableArray<DirectCommandMetadata?> directCommands,
        SourceProductionContext context
    )
    {
        List<ServiceMetadata> servicesToGenerate = new();
        foreach (ClassDeclarationSyntax? classDecl in serviceClasses.Distinct())
        {
            SemanticModel model = compilation.GetSemanticModel(classDecl!.SyntaxTree);
            INamedTypeSymbol? symbol = model.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
            if (symbol == null)
                continue;

            servicesToGenerate.Add(
                new ServiceMetadata
                {
                    ClassName = symbol.Name,
                    InterfaceName = symbol.Interfaces.FirstOrDefault()?.Name ?? $"I{symbol.Name}",
                    Namespace = symbol.ContainingNamespace.ToDisplayString(),
                    Methods = symbol
                        .GetMembers()
                        .OfType<IMethodSymbol>()
                        .Where(m =>
                            m.DeclaredAccessibility == Accessibility.Public
                            && m.MethodKind == MethodKind.Ordinary
                        )
                        .Select(m => new MethodMetadata
                        {
                            MethodName = m.Name,
                            ResponseType =
                                (m.ReturnType as INamedTypeSymbol)?.IsGenericType == true
                                    ? (m.ReturnType as INamedTypeSymbol)
                                        .TypeArguments[0]
                                        .ToDisplayString()
                                    : "Unit",
                            Parameters = m
                                .Parameters.Select(p => new ParameterMetadata
                                {
                                    Name = p.Name,
                                    Type = p.Type.ToDisplayString(),
                                })
                                .ToList(),
                        })
                        .ToList(),
                }
            );
        }
        GenerateCode(context, servicesToGenerate, directCommands.ToList()!);
    }

    private void GenerateCode(
        SourceProductionContext context,
        List<ServiceMetadata> services,
        List<DirectCommandMetadata> directCommands
    )
    {
        StringBuilder sb = new();
        sb.AppendLine("// <auto-generated />");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Threading;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using LinKit.Core.Cqrs;");
        sb.AppendLine("using Grpc.Core;");
        sb.AppendLine("using O24OpenAPI.GrpcContracts.Factory;");
        sb.AppendLine("using O24OpenAPI.Core.Infrastructure;");
        sb.AppendLine("using O24OpenAPI.Core.Configuration;");
        sb.AppendLine("using O24OpenAPI.GrpcContracts.Extensions;");

        IOrderedEnumerable<string> distinctNamespaces = services
            .Select(s => s.Namespace)
            .Distinct()
            .OrderBy(n => n);
        foreach (var ns in distinctNamespaces)
            sb.AppendLine($"using {ns};");

        sb.AppendLine("\nnamespace O24OpenAPI.GrpcClient.Generated");
        sb.AppendLine("{");
        foreach (ServiceMetadata service in services)
            foreach (MethodMetadata method in service.Methods)
                sb.AppendLine(
                    $"    public record {service.ClassName}{method.MethodName}Command({string.Join(", ", method.Parameters.Select(p => $"{p.Type} {Capitalize(p.Name)}"))}) : ICommand<{method.ResponseType}>;"
                );
        sb.AppendLine("}");

        sb.AppendLine("\nnamespace O24OpenAPI.Grpc.Mediator.Generated");
        sb.AppendLine("{");
        sb.AppendLine("    public class GrpcMediator : IMediator");
        sb.AppendLine("    {");
        sb.AppendLine("        private readonly IServiceProvider _serviceProvider;");
        sb.AppendLine(
            "        public GrpcMediator(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;"
        );

        sb.AppendLine(
            "\n        public async Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken ct = default)"
        );
        sb.AppendLine("        {");
        sb.AppendLine(
            "            var _grpcClientFactory = _serviceProvider.GetRequiredService<IGrpcClientFactory>();"
        );
        sb.AppendLine(
            "            var _defaultHeader = new Metadata { { \"flow\", $\"{Singleton<O24OpenAPIConfiguration>.Instance.YourServiceID} -> External\" } };"
        );

        // CHUYỂN TOÀN BỘ SANG SWITCH STATEMENT
        sb.AppendLine("\n            switch (command)");
        sb.AppendLine("            {");

        // Nhánh 1: Gọi qua Service cũ
        foreach (ServiceMetadata service in services)
        {
            foreach (MethodMetadata method in service.Methods)
            {
                var cmdType =
                    $"O24OpenAPI.GrpcClient.Generated.{service.ClassName}{method.MethodName}Command";
                var args = string.Join(
                    ", ",
                    method.Parameters.Select(p => $"c.{Capitalize(p.Name)}")
                );
                sb.AppendLine($"                case {cmdType} c:");
                sb.AppendLine(
                    $"                    var result_{service.ClassName}_{method.MethodName} = await _serviceProvider.GetRequiredService<{service.InterfaceName}>().{method.MethodName}({args});"
                );
                sb.AppendLine(
                    $"                    return (TResponse)(object)result_{service.ClassName}_{method.MethodName};"
                );
            }
        }

        // Nhánh 2: Gọi trực tiếp gRPC (Attribute logic)
        foreach (DirectCommandMetadata direct in directCommands)
        {
            var cmdFullType = direct.CommandSymbol.ToDisplayString();
            sb.AppendLine($"                case {cmdFullType} c:");
            sb.AppendLine("                    {");
            sb.AppendLine(
                $"                        var client = await _grpcClientFactory.GetClientAsync<{direct.GrpcClientFullType}>();"
            );
            sb.AppendLine(
                $"                        var request = c.To{direct.RequestFullType.Substring(direct.RequestFullType.LastIndexOf('.') + 1)}();"
            );
            sb.AppendLine(
                $"                        var callResult = await client.{direct.MethodName}(request, _defaultHeader).CallAsync<{direct.ResponseType}>();"
            );
            sb.AppendLine("                        return (TResponse)(object)callResult;");
            sb.AppendLine("                    }");
        }

        sb.AppendLine("                default:");
        sb.AppendLine(
            "                    throw new NotImplementedException($\"Command {command.GetType().Name} not registered in GrpcMediator.\");"
        );
        sb.AppendLine("            }");
        sb.AppendLine("        }");

        sb.AppendLine(
            "\n        public Task SendAsync(ICommand command, CancellationToken ct = default) => throw new NotImplementedException();"
        );
        sb.AppendLine(
            "        public Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken ct = default) => throw new NotImplementedException();"
        );
        sb.AppendLine(
            "\n        public Task PublishAsync<TNotification>(TNotification notification, PublishStrategy strategy = PublishStrategy.Sequential, CancellationToken ct = default) => throw new NotImplementedException();"
        );
        sb.AppendLine("    }");

        sb.AppendLine("\n    public static class GrpcMediatorExtensions");
        sb.AppendLine("    {");
        sb.AppendLine(
            "        public static IServiceCollection AddGrpcMediator(this IServiceCollection services)"
        );
        sb.AppendLine("        {");
        sb.AppendLine("            services.AddKeyedScoped<IMediator, GrpcMediator>(\"grpc\");");
        sb.AppendLine("            return services;");
        sb.AppendLine("        }");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        context.AddSource("GrpcMediator.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
    }

    private string Capitalize(string s) =>
        string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1);

    private class ServiceMetadata
    {
        public string ClassName { get; set; }
        public string InterfaceName { get; set; }
        public string Namespace { get; set; }
        public List<MethodMetadata> Methods { get; set; }
    }

    private class MethodMetadata
    {
        public string MethodName { get; set; }
        public string ResponseType { get; set; }
        public List<ParameterMetadata> Parameters { get; set; }
    }

    private class ParameterMetadata
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    private class DirectCommandMetadata
    {
        public INamedTypeSymbol CommandSymbol { get; set; }
        public string GrpcClientFullType { get; set; }
        public string MethodName { get; set; }
        public string RequestFullType { get; set; }
        public string ResponseType { get; set; }
        public List<string> Properties { get; set; }
    }
}
