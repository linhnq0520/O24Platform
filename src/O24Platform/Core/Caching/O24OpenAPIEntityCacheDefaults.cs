namespace O24OpenAPI.Core.Caching;

/// <summary>
/// The 24 open api entity cache defaults class
/// </summary>
public static class O24OpenAPIEntityCacheDefaults<TEntity>
    where TEntity : BaseEntity
{
    /// <summary>
    /// Gets the value of the entity type name
    /// </summary>
    public static string EntityTypeName => typeof(TEntity).FullName!.ToLowerInvariant();

    /// <summary>
    /// Gets the value of the by code prefix
    /// </summary>
    public static string ByCodePrefix => new PrefixKey(EntityTypeName + ".bycode.").Key;

    /// <summary>
    /// Gets the value of the prefix
    /// </summary>
    public static string Prefix
    {
        get => "O24." + EntityTypeName + ".";
    }

    /// <summary>
    /// Gets the value of the by id prefix
    /// </summary>
    public static string ByIdPrefix
    {
        get => "O24." + EntityTypeName + ".byid.";
    }

    /// <summary>
    /// Gets the value of the by ids prefix
    /// </summary>
    public static string ByIdsPrefix
    {
        get => "O24." + EntityTypeName + ".byids.";
    }

    /// <summary>
    /// Gets the value of the all prefix
    /// </summary>
    public static string AllPrefix
    {
        get => "O24." + EntityTypeName + ".all.";
    }
}
