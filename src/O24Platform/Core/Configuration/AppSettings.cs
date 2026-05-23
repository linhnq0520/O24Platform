using System.Runtime.CompilerServices;

namespace O24OpenAPI.Core.Configuration;

/// <summary>
/// The app settings class
/// </summary>
public class AppSettings
{
    /// <summary>
    /// The config
    /// </summary>
    private readonly Dictionary<Type, IConfig> _configurations = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="AppSettings"/> class
    /// </summary>
    /// <param name="configurations">The configurations</param>
    public AppSettings(IList<IConfig>? configurations = null)
    {
        Dictionary<Type, IConfig>? dictionary;
        if (configurations == null)
        {
            dictionary = null;
        }
        else
        {
            IOrderedEnumerable<IConfig> source = configurations.OrderBy(config =>
                config.GetOrder()
            );
            dictionary = source?.ToDictionary(config => config.GetType(), config => config);
        }
        dictionary ??= [];
        this._configurations = dictionary;
    }

    /// <summary>
    /// Gets this instance
    /// </summary>
    /// <typeparam name="TConfig">The config</typeparam>
    /// <exception cref="O24OpenAPIException"></exception>
    /// <returns>The configuration</returns>
    public TConfig? Get<TConfig>()
        where TConfig : class, IConfig
    {
        if (
            !_configurations.TryGetValue(typeof(TConfig), out IConfig? configuration)
            || configuration is null
            || configuration is not TConfig config
        )
        {
            DefaultInterpolatedStringHandler interpolatedStringHandler = new(35, 1);
            interpolatedStringHandler.AppendLiteral("No configuration with type '");
            interpolatedStringHandler.AppendFormatted<Type>(typeof(TConfig));
            interpolatedStringHandler.AppendLiteral("' found");
            return null;
            //throw new O24OpenAPIException(interpolatedStringHandler.ToStringAndClear());
        }
        return config;
    }

    /// <summary>
    /// Updates the configurations
    /// </summary>
    /// <param name="configurations">The configurations</param>
    public void Update(IList<IConfig> configurations)
    {
        foreach (IConfig configuration in (IEnumerable<IConfig>)configurations)
        {
            this._configurations[configuration.GetType()] = configuration;
        }
    }
}
