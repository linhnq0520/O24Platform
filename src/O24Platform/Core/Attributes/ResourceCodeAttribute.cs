namespace O24OpenAPI.Core.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class StringResourceAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
public class LocalizedAttribute : Attribute
{
    public string Lang { get; }
    public string Value { get; }

    public LocalizedAttribute(string lang, string value)
    {
        Lang = lang;
        Value = value;
    }
}
