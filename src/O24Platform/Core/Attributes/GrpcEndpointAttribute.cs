namespace O24OpenAPI.Core.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class GrpcEndpointAttribute : Attribute
{
    public Type ServiceBaseType { get; }

    public string MethodName { get; }
    public string? MediatorKey { get; set; }

    public GrpcEndpointAttribute(Type serviceBaseType, string methodName)
    {
        ServiceBaseType = serviceBaseType;
        MethodName = methodName;
    }
}
