namespace O24OpenAPI.Framework.Attributes;

/// <summary>
/// Skips response wrapping for this endpoint. Works on controllers and minimal APIs.
/// </summary>
[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = false,
    Inherited = true
)]
public sealed class SkipResponseWrapperAttribute : Attribute;
