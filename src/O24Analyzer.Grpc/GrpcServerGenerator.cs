using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace O24OpenAPI.Grpc.Generator;

[Generator]
public class GrpcServerGenerator : IIncrementalGenerator
{
    private const string GrpcEndpointAttributeName =
        "O24OpenAPI.Core.Attributes.GrpcEndpointAttribute";
    private const string IQueryInterfaceName = "LinKit.Core.Cqrs.IQuery";
    private const string ICommandInterfaceName = "LinKit.Core.Cqrs.ICommand";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<ServerTransformationResult> grpcDeclarations =
            context.SyntaxProvider.ForAttributeWithMetadataName(
                GrpcEndpointAttributeName,
                predicate: (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax,
                transform: (ctx, _) => GetGrpcEndpointInfo(ctx)
            );

        IncrementalValuesProvider<Diagnostic> diagnostics = grpcDeclarations
            .Where(x => x.Diagnostic is not null)
            .Select((x, _) => x.Diagnostic!);

        context.RegisterSourceOutput(
            diagnostics,
            (spc, diagnostic) => spc.ReportDiagnostic(diagnostic)
        );

        IncrementalValuesProvider<GrpcEndpointInfo> validEndpoints = grpcDeclarations
            .Where(x => x.EndpointInfo is not null)
            .Select((x, _) => x.EndpointInfo!);

        context.RegisterSourceOutput(
            validEndpoints.Collect(),
            (spc, endpoints) =>
            {
                if (endpoints.IsEmpty)
                {
                    return;
                }
                var source = GenerateGrpcServices(endpoints);
                spc.AddSource("Grpc.Services.g.cs", SourceText.From(source, Encoding.UTF8));
            }
        );
    }

    private static ServerTransformationResult GetGrpcEndpointInfo(
        GeneratorAttributeSyntaxContext context
    )
    {
        if (context.TargetSymbol is not INamedTypeSymbol cqrsRequestSymbol)
        {
            return new ServerTransformationResult();
        }

        AttributeData attributeData = context.Attributes[0];
        Location attributeLocation =
            attributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation() ?? Location.None;

        string mediatorKey = string.Empty;
        KeyValuePair<string, TypedConstant> mediatorKeyArgument = attributeData.NamedArguments.FirstOrDefault(kvp =>
            kvp.Key == "MediatorKey"
        );
        if (!mediatorKeyArgument.Value.IsNull)
        {
            mediatorKey = mediatorKeyArgument.Value.Value?.ToString() ?? string.Empty;
        }

        if (
            attributeData.ConstructorArguments.Length < 2
            || attributeData.ConstructorArguments[0].Value is not INamedTypeSymbol serviceBaseSymbol
            || attributeData.ConstructorArguments[1].Value is not string methodName
        )
        {
            Diagnostic diag = Diagnostic.Create(
                ServerDiagnostics.LKG104_InvalidAttributeUsage,
                attributeLocation
            );
            return new ServerTransformationResult { Diagnostic = diag };
        }

        IMethodSymbol? rpcMethod = serviceBaseSymbol
            .GetMembers(methodName)
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m =>
                m.Name == methodName
                && !m.IsStatic
                && m.Parameters.Length == 2
                && m.ReturnType.Name == "Task"
            );

        if (rpcMethod is null)
        {
            Diagnostic diag = Diagnostic.Create(
                ServerDiagnostics.LKG101_MethodNotFound,
                attributeLocation,
                methodName,
                serviceBaseSymbol.Name
            );
            return new ServerTransformationResult { Diagnostic = diag };
        }

        var grpcRequestSymbol = rpcMethod.Parameters[0].Type as INamedTypeSymbol;
        var grpcResponseSymbol =
            (rpcMethod.ReturnType as INamedTypeSymbol)?.TypeArguments.FirstOrDefault()
            as INamedTypeSymbol;

        if (
            grpcRequestSymbol is null
            || grpcResponseSymbol is null
            || rpcMethod.Parameters[1].Type.Name != "ServerCallContext"
        )
        {
            Diagnostic diag = Diagnostic.Create(
                ServerDiagnostics.LKG102_InvalidMethodSignature,
                attributeLocation,
                methodName
            );
            return new ServerTransformationResult { Diagnostic = diag };
        }

        INamedTypeSymbol? cqrsInterface = cqrsRequestSymbol.AllInterfaces.FirstOrDefault(i =>
            i.ToDisplayString().StartsWith(ICommandInterfaceName)
            || // ICommand...
            i.ToDisplayString().StartsWith(IQueryInterfaceName) // IQuery...
        );

        if (cqrsInterface is null)
        {
            Diagnostic diag = Diagnostic.Create(
                ServerDiagnostics.LKG103_MissingCqrsInterface,
                context.TargetNode.GetLocation(),
                cqrsRequestSymbol.Name
            );
            return new ServerTransformationResult { Diagnostic = diag };
        }

        string cqrsResponseType;
        bool isCommandWithoutResult = false;
        INamedTypeSymbol? cqrsResponseSymbol = null;

        if (cqrsInterface.TypeArguments.Length > 0)
        {
            cqrsResponseSymbol = cqrsInterface.TypeArguments[0] as INamedTypeSymbol;
            if (cqrsResponseSymbol is null)
            {
                // Không thể phân giải kiểu generic T từ ICommand<T>
                Diagnostic diag = Diagnostic.Create(
                    ServerDiagnostics.LKG105_UnresolvedResponseType,
                    context.TargetNode.GetLocation(),
                    cqrsRequestSymbol.Name
                );
                return new ServerTransformationResult { Diagnostic = diag };
            }
            cqrsResponseType = cqrsResponseSymbol.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
            );
        }
        else
        {
            cqrsResponseType = "System.ValueTuple";
            isCommandWithoutResult = true;
        }

        List<PropertyMap> requestMaps = GetPropertyMaps(cqrsRequestSymbol, grpcRequestSymbol);
        (List<PropertyMap>? responseMaps, List<ListPropertyMap>? responseListMaps) = GetResponseMaps(
            cqrsResponseSymbol,
            grpcResponseSymbol
        );
        var grpcResponseDtoType = grpcResponseSymbol.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat
        );
        bool isQuery = cqrsInterface
            .OriginalDefinition.ToDisplayString()
            .StartsWith(IQueryInterfaceName);

        GrpcEndpointInfo endpointInfo = new(
            CqrsRequestType: cqrsRequestSymbol.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
            ),
            CqrsResponseType: cqrsResponseType,
            ServiceBaseType: serviceBaseSymbol.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
            ),
            GrpcMethodName: methodName,
            GrpcRequestType: grpcRequestSymbol.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
            ),
            GrpcResponseType: grpcResponseSymbol.ToDisplayString(
                SymbolDisplayFormat.FullyQualifiedFormat
            ),
            RequestPropertyMaps: requestMaps,
            ResponsePropertyMaps: responseMaps,
            ResponseListPropertyMaps: responseListMaps,
            GrpcResponseDtoProperty: string.Empty,
            GrpcResponseDtoType: grpcResponseDtoType,
            IsCqrsQuery: isQuery,
            IsCommandWithoutResult: isCommandWithoutResult,
            MediatorKey: mediatorKey
        );

        return new ServerTransformationResult { EndpointInfo = endpointInfo };
    }

    private static (List<PropertyMap>, List<ListPropertyMap>) GetResponseMaps(
        INamedTypeSymbol? source,
        INamedTypeSymbol? destination
    )
    {
        if (source is null || destination is null)
        {
            return ([], []);
        }

        var sourceProps = source
            .GetMembers()
            .OfType<IPropertySymbol>()
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        var destProps = destination
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.SetMethod is not null || IsRepeatedType(p.Type))
            .ToList();

        var simpleMaps = new List<PropertyMap>();
        var listMaps = new List<ListPropertyMap>();

        foreach (IPropertySymbol? destProp in destProps)
        {
            if (sourceProps.TryGetValue(destProp.Name, out IPropertySymbol? sourceProp))
            {
                bool isSourceList = IsListType(sourceProp.Type);
                bool isDestList = IsRepeatedType(destProp.Type);

                if (isSourceList && isDestList)
                {
                    INamedTypeSymbol? sourceItemType = GetListItemType(sourceProp.Type);
                    INamedTypeSymbol? destItemType = GetRepeatedItemType(destProp.Type);
                    if (sourceItemType != null && destItemType != null)
                    {
                        List<PropertyMap> itemMaps = GetPropertyMaps(sourceItemType, destItemType);
                        listMaps.Add(
                            new ListPropertyMap(
                                SourceProperty: sourceProp.Name,
                                DestProperty: destProp.Name,
                                SourceItemType: sourceItemType.ToDisplayString(
                                    SymbolDisplayFormat.FullyQualifiedFormat
                                ),
                                DestItemType: destItemType.ToDisplayString(
                                    SymbolDisplayFormat.FullyQualifiedFormat
                                ),
                                ItemPropertyMaps: itemMaps
                            )
                        );
                    }
                }
                else if (!isSourceList && !isDestList)
                {
                    simpleMaps.Add(new PropertyMap(sourceProp.Name, destProp.Name));
                }
            }
        }

        return (simpleMaps, listMaps);
    }

    private static List<PropertyMap> GetPropertyMaps(
        INamedTypeSymbol? source,
        INamedTypeSymbol? destination
    )
    {
        if (source is null || destination is null)
        {
            return [];
        }

        static IEnumerable<IPropertySymbol> GetAllProperties(INamedTypeSymbol type)
        {
            INamedTypeSymbol? current = type;
            while (current != null)
            {
                foreach (IPropertySymbol member in current.GetMembers().OfType<IPropertySymbol>())
                {
                    yield return member;
                }
                current = current.BaseType;
            }
        }

        var sourceProps = GetAllProperties(source)
            .Where(p => p.GetMethod is not null)
            .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        var destProps = GetAllProperties(destination)
            .Where(p => p.SetMethod is not null)
            .GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        var maps = new List<PropertyMap>();
        foreach (IPropertySymbol? destProp in destProps)
        {
            if (sourceProps.TryGetValue(destProp.Name, out IPropertySymbol? sourceProp))
            {
                maps.Add(new PropertyMap(sourceProp.Name, destProp.Name));
            }
        }

        return maps;
    }

    private static bool IsListType(ITypeSymbol type)
    {
        return type is INamedTypeSymbol namedType
            && (
                namedType
                    .ConstructedFrom.ToDisplayString()
                    .StartsWith("System.Collections.Generic.List<")
                || namedType
                    .ConstructedFrom.ToDisplayString()
                    .StartsWith("System.Collections.Generic.IList<")
                || namedType
                    .ConstructedFrom.ToDisplayString()
                    .StartsWith("System.Collections.Generic.IEnumerable<")
            );
    }

    private static INamedTypeSymbol? GetListItemType(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType && namedType.TypeArguments.Length > 0)
        {
            return namedType.TypeArguments[0] as INamedTypeSymbol;
        }
        return null;
    }

    private static bool IsRepeatedType(ITypeSymbol type)
    {
        return type is INamedTypeSymbol namedType
            && namedType.ConstructedFrom.ToDisplayString().Contains("RepeatedField<");
    }

    private static INamedTypeSymbol? GetRepeatedItemType(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType && namedType.TypeArguments.Length > 0)
        {
            return namedType.TypeArguments[0] as INamedTypeSymbol;
        }
        return null;
    }

    private static string GenerateGrpcServices(IReadOnlyList<GrpcEndpointInfo> endpoints)
    {
        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated> by LinKit.Generator");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using Grpc.Core;");
        sb.AppendLine("using LinKit.Core.Cqrs;");
        sb.AppendLine("using LinKit.Core.Abstractions;");
        sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
        sb.AppendLine("using Microsoft.Extensions.Logging;");
        sb.AppendLine("using System;");
        sb.AppendLine("using System.ComponentModel.DataAnnotations;");
        sb.AppendLine("using System.Threading.Tasks;");
        sb.AppendLine("using O24OpenAPI.GrpcContracts.Extensions;");
        sb.AppendLine();

        IEnumerable<IGrouping<string, GrpcEndpointInfo>> endpointsByService = endpoints.GroupBy(e => e.ServiceBaseType);

        foreach (IGrouping<string, GrpcEndpointInfo>? serviceGroup in endpointsByService)
        {
            var serviceBaseType = serviceGroup.Key;
            var baseClassName = serviceBaseType.Split('.').Last();
            var generatedClassName = $"LinKit{baseClassName.Replace("Base", "")}";

            var namespaceParts = serviceBaseType.Split('.');
            var nsWithGlobal = string.Join(".", namespaceParts.Take(namespaceParts.Length - 2));
            if (string.IsNullOrEmpty(nsWithGlobal))
            {
                nsWithGlobal = "LinKit.Generated.Grpc";
            }

            var ns = nsWithGlobal.StartsWith("global::")
                ? nsWithGlobal.Substring("global::".Length)
                : nsWithGlobal;

            sb.AppendLine($"namespace {ns}");
            sb.AppendLine("{");
            sb.AppendLine($"    public sealed class {generatedClassName} : {serviceBaseType}");
            sb.AppendLine("    {");
            sb.AppendLine("        private readonly IMediator _defaultMediator;");
            sb.AppendLine("        private readonly IServiceProvider _serviceProvider;");
            sb.AppendLine($"        private readonly ILogger<{generatedClassName}>? _logger;");
            sb.AppendLine();

            sb.AppendLine(
                $"        public {generatedClassName}(IMediator mediator, IServiceProvider serviceProvider)"
            );
            sb.AppendLine("        {");
            sb.AppendLine("            _defaultMediator = mediator;");
            sb.AppendLine("            _serviceProvider = serviceProvider;");

            sb.AppendLine(
                $"            _logger = serviceProvider.GetService<ILogger<{generatedClassName}>>();"
            );
            sb.AppendLine("        }");
            sb.AppendLine();

            foreach (GrpcEndpointInfo? endpoint in serviceGroup)
            {
                var requestMappings = endpoint.RequestPropertyMaps.Any()
                    ? $" {{ {string.Join(", ",
        endpoint.RequestPropertyMaps.Select(
            m => $"{m.SourceProperty} = request.{m.DestProperty}"
        ))} }}"
                    : "()";

                var mediatorMethod = endpoint.IsCqrsQuery ? "QueryAsync" : "SendAsync";
                var cqrsResponseIsNullable = endpoint.CqrsResponseType.EndsWith("?");

                sb.AppendLine(
                    $"        public override async Task<{endpoint.GrpcResponseType}> {endpoint.GrpcMethodName}({endpoint.GrpcRequestType} request, ServerCallContext context)"
                );
                sb.AppendLine("        {");
                sb.AppendLine("            try");
                sb.AppendLine("            {");
                if (!string.IsNullOrEmpty(endpoint.MediatorKey))
                {
                    sb.AppendLine(
                        $"                var mediator = _serviceProvider.GetRequiredKeyedService<IMediator>(\"{endpoint.MediatorKey}\");"
                    );
                }
                else
                {
                    sb.AppendLine("                var mediator = _defaultMediator;");
                }
                sb.AppendLine(
                    $"                var cqrsRequest = new {endpoint.CqrsRequestType}{requestMappings};"
                );
                sb.AppendLine(
                    endpoint.IsCommandWithoutResult
                        ? $"                await _mediator.SendAsync(cqrsRequest, context.CancellationToken);"
                        : $"                var cqrsResult = mediator.{mediatorMethod}<{endpoint.CqrsResponseType.TrimEnd('?')}>(cqrsRequest, context.CancellationToken);"
                );
                if (cqrsResponseIsNullable)
                {
                    sb.AppendLine("                if (cqrsResult is null)");
                    sb.AppendLine("                {");
                    sb.AppendLine(
                        "                    throw new RpcException(new Status(StatusCode.NotFound, \"Resource not found.\"));"
                    );
                    sb.AppendLine("                }");
                }
                sb.AppendLine($"                return await cqrsResult.GetGrpcResponseAsync();");

                sb.AppendLine("            }");
                sb.AppendLine("            catch (ValidationException ex)");
                sb.AppendLine("            {");
                sb.AppendLine(
                    "                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));"
                );
                sb.AppendLine("            }");
                sb.AppendLine("            catch (Exception ex)");
                sb.AppendLine("            {");
                sb.AppendLine(
                    $"                _logger?.LogError(ex, \"An unexpected error occurred while handling {endpoint.GrpcMethodName}.\");"
                );
                sb.AppendLine(
                    @"                throw new RpcException(new Status(StatusCode.Internal, ""An internal error occurred.""));"
                );
                sb.AppendLine("            }");
                sb.AppendLine("        }");
                sb.AppendLine();
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static IncrementalValueProvider<IReadOnlyList<GrpcServiceInfo>> GetServices(
        IncrementalGeneratorInitializationContext context
    )
    {
        IncrementalValuesProvider<GrpcEndpointInfo> grpcDeclarations = context
            .SyntaxProvider.ForAttributeWithMetadataName(
                GrpcEndpointAttributeName,
                predicate: (node, _) => node is ClassDeclarationSyntax or RecordDeclarationSyntax,
                transform: (ctx, _) => GetGrpcEndpointInfo(ctx)
            )
            .Where(info => info is not null)
            .Select((x, _) => x!.EndpointInfo!);

        return grpcDeclarations
            .Collect()
            .Select(
                (endpoints, _) =>
                {
                    var services = new List<GrpcServiceInfo>();

                    return (IReadOnlyList<GrpcServiceInfo>)services;
                }
            );
    }
}

internal record GrpcEndpointInfo(
    string CqrsRequestType,
    string CqrsResponseType,
    string ServiceBaseType,
    string GrpcMethodName,
    string GrpcRequestType,
    string GrpcResponseType,
    IReadOnlyList<PropertyMap> RequestPropertyMaps,
    IReadOnlyList<PropertyMap> ResponsePropertyMaps,
    IReadOnlyList<ListPropertyMap> ResponseListPropertyMaps,
    string? GrpcResponseDtoProperty,
    string? GrpcResponseDtoType,
    bool IsCqrsQuery,
    bool IsCommandWithoutResult,
    string MediatorKey = ""
);

internal record PropertyMap(string SourceProperty, string DestProperty);

internal record ListPropertyMap(
    string SourceProperty,
    string DestProperty,
    string SourceItemType,
    string DestItemType,
    IReadOnlyList<PropertyMap> ItemPropertyMaps
);

internal record ServerTransformationResult
{
    public GrpcEndpointInfo? EndpointInfo { get; init; }
    public Diagnostic? Diagnostic { get; init; }
}

internal record GrpcServiceInfo(string RegistrationCode);
