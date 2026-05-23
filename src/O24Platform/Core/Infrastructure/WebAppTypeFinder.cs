using System.Reflection;

namespace O24OpenAPI.Core.Infrastructure;

/// <summary>
/// The web app type finder class
/// </summary>
/// <seealso cref="AppDomainTypeFinder"/>
public class WebAppTypeFinder : AppDomainTypeFinder
{
    /// <summary>
    /// The bin folder assemblies loaded
    /// </summary>
    private bool _binFolderAssembliesLoaded;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebAppTypeFinder"/> class
    /// </summary>
    /// <param name="fileProvider">The file provider</param>
    public WebAppTypeFinder(IO24OpenAPIFileProvider? fileProvider = null)
        : base(fileProvider) { }

    /// <summary>
    /// Gets or sets the value of the ensure bin folder assemblies loaded
    /// </summary>
    public bool EnsureBinFolderAssembliesLoaded { get; set; } = true;

    /// <summary>
    /// Gets the bin directory
    /// </summary>
    /// <returns>The string</returns>
    public virtual string GetBinDirectory() => AppContext.BaseDirectory;

    /// <summary>
    /// Gets the assemblies
    /// </summary>
    /// <returns>A list of assembly</returns>
    public override IList<Assembly> GetAssemblies()
    {
        if (!this.EnsureBinFolderAssembliesLoaded || this._binFolderAssembliesLoaded)
        {
            return base.GetAssemblies();
        }

        this._binFolderAssembliesLoaded = true;
        this.LoadMatchingAssemblies(this.GetBinDirectory());
        return base.GetAssemblies();
    }
}
