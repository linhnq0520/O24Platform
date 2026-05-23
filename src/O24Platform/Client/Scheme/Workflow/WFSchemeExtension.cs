namespace O24OpenAPI.Client.Scheme.Workflow;

public static class WFSchemeExtension
{
    public static WFScheme ToWFScheme(this string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<WFScheme>(json);
        }
        catch (System.Text.Json.JsonException ex)
        {
            throw new InvalidOperationException("Failed to deserialize JSON to WFScheme.", ex);
        }
    }
}
