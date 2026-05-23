namespace O24OpenAPI.Core.Attributes;

/// <summary>
/// /// The environment type enum
/// </summary>
public enum EnvironmentType
{
    /// <summary>
    /// The dev environment type
    /// </summary>
    Dev,

    /// <summary>
    /// The test environment type
    /// </summary>
    Test,

    /// <summary>
    /// The uat environment type
    /// </summary>
    UAT,

    /// <summary>
    /// The prod environment type
    /// </summary>
    Prod,

    /// <summary>
    /// The all environment type
    /// </summary>
    All,
}

/// <summary>
/// The environment attribute class
/// </summary>
/// <seealso cref="Attribute"/>
public class EnvironmentAttribute : Attribute
{
    /// <summary>
    /// Gets the value of the environment
    /// </summary>
    public EnvironmentType Environment { get; }

    /// <summary>
    /// Gets the value of the environment types
    /// </summary>
    public IEnumerable<EnvironmentType> EnvironmentTypes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentAttribute"/> class
    /// </summary>
    /// <param name="environment">The environment</param>
    public EnvironmentAttribute(EnvironmentType environment)
    {
        Environment = environment;
        EnvironmentTypes = [environment];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EnvironmentAttribute"/> class
    /// </summary>
    /// <param name="environments">The environments</param>
    public EnvironmentAttribute(params EnvironmentType[] environments)
    {
        EnvironmentTypes = environments;
    }
}
