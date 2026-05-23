namespace O24OpenAPI.Framework.Models;

public partial class SettingSearchModel : BaseTransactionModel
{
    /// <summary>
    ///
    /// </summary>
    public SettingSearchModel() { }

    /// <summary>
    ///
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    ///
    /// </summary>
    public string OrganizationId { get; set; }

    /// <summary>
    ///
    /// </summary>
    public int PageIndex { get; set; } = 0;

    /// <summary>
    ///
    /// </summary>
    public int PageSize { get; set; } = int.MaxValue;
}

/// <summary>
///
/// </summary>
public partial class SettingSearchResponse : BaseO24OpenAPIModel
{
    /// <summary>
    ///
    /// </summary>
    public SettingSearchResponse() { }

    /// <summary>
    ///
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    ///
    /// </summary>
    public string OrganizationId { get; set; }
}

/// <summary>
///
/// </summary>
public partial class SettingViewByPrimaryKey : BaseTransactionModel
{
    /// <summary>
    ///
    /// </summary>
    public SettingViewByPrimaryKey() { }

    /// <summary>
    ///
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///
    /// </summary>
    public int OrganizationId { get; set; } = 0;
}

/// <summary>
///
/// </summary>
public partial class SettingCreateModel : BaseTransactionModel
{
    /// <summary>
    ///
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    ///
    /// </summary>
    public int OrganizationId { get; set; }
}

/// <summary>
///
/// </summary>
public partial class SettingUpdateModel : BaseTransactionModel
{
    /// <summary>
    ///
    /// </summary>
    public SettingUpdateModel() { }

    /// <summary>
    ///
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    ///
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    ///
    /// </summary>
    public int OrganizationId { get; set; }
}
