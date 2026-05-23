namespace O24OpenAPI.Core.Extensions;

/// <summary>
/// The model extensions class
/// </summary>
public static class ModelExtensions
{
    /// <summary>
    /// Describes whether has id
    /// </summary>
    /// <param name="objectModel">The object model</param>
    /// <returns>The bool</returns>
    public static bool HasId(this object objectModel) => objectModel.HasMethod("Id");

    /// <summary>
    /// Describes whether has method
    /// </summary>
    /// <param name="objectModel">The object model</param>
    /// /// <param name="propertyName">The property name</param>
    /// <returns>The bool</returns>
    public static bool HasMethod(this object objectModel, string propertyName) =>
        objectModel.GetType().GetProperty(propertyName) != null;
}
