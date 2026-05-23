namespace O24OpenAPI.Framework.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class WorkflowStepAttribute(string stepCode) : Attribute
{
    public string StepCode { get; } = stepCode;
}
