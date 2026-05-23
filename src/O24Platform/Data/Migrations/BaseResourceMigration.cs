using O24OpenAPI.Core.Domain.Localization;
using O24OpenAPI.Core.Helper;

namespace O24OpenAPI.Data.Migrations;

public abstract class BaseResourceMigration : BaseMigration
{
    protected Task MigrateUp(Type resourceType, string? fieldName = null)
    {
        List<LocaleStringResource> list;
        if (!string.IsNullOrWhiteSpace(fieldName))
        {
            list = ResourceReflectionHelper.ExtractField(resourceType, fieldName);
        }
        else
        {
            list = ResourceReflectionHelper.Extract(resourceType);
        }
        return SeedListData(list, [nameof(LocaleStringResource.ResourceName)]);
    }
}
