namespace O24OpenAPI.Data.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class DatabaseTypeAttribute(params DataProviderType[] databaseTypes) : Attribute
{
    public DataProviderType[] DatabaseTypes { get; } = databaseTypes;
}
