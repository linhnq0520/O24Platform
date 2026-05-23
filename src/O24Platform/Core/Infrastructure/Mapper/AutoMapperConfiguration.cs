using AutoMapper;

namespace O24OpenAPI.Core.Infrastructure.Mapper;

/// <summary>
/// The auto mapper configuration class
/// </summary>
public static class AutoMapperConfiguration
{
    /// <summary>
    /// Gets or sets the value of the mapper
    /// </summary>
    public static IMapper? Mapper { get; private set; }

    /// <summary>
    /// Gets or sets the value of the mapper configuration
    /// </summary>
    public static MapperConfiguration? MapperConfiguration { get; private set; }

    /// <summary>
    /// Inits the config
    /// </summary>
    /// <param name="config">The config</param>
    public static void Init(MapperConfiguration config)
    {
        MapperConfiguration = config;
        Mapper = config.CreateMapper();
    }
}
