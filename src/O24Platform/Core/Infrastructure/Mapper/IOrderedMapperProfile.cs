namespace O24OpenAPI.Core.Infrastructure.Mapper;

/// <summary>
/// The ordered mapper profile interface
/// </summary>
public interface IOrderedMapperProfile
{
    /// <summary>
    /// Gets the value of the order
    /// </summary>
    int Order { get; }
}
