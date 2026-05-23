namespace O24OpenAPI.Framework.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class LearnApiAttribute(string learnApiId) : Attribute
{
    public string LearnApiId { get; } = learnApiId;
}
