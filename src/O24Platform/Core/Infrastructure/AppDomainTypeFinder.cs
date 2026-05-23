using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using O24OpenAPI.Core.Helper;

namespace O24OpenAPI.Core.Infrastructure;

/// <summary>
/// The app domain type finder class
/// </summary>
/// <seealso cref="ITypeFinder"/>
public class AppDomainTypeFinder : ITypeFinder
{
    /// <summary>
    /// The ignore reflection errors
    /// /// </summary>
    private bool _ignoreReflectionErrors = true;

    /// <summary>
    /// The file provider
    /// </summary>
    protected IO24OpenAPIFileProvider? _fileProvider;

    /// <summary>
    /// Finds the classes of type using the specified only concrete classes
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="onlyConcreteClasses">The only concrete classes</param>
    /// <returns>An enumerable of type</returns>
    public IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true)
    {
        return FindClassesOfType(typeof(T), onlyConcreteClasses);
    }

    /// <summary>
    /// Finds the class of type using the specified entity name
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="entityName">The entity name</param>
    /// <returns>The type</returns>
    public Type? FindClassOfType<T>(string entityName)
    {
        var types = FindClassesOfType<T>();
        return types.FirstOrDefault(t =>
            t.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase)
        );
    }

    /// <summary>
    /// Finds the classes of type using the specified assign type from
    /// </summary>
    /// <param name="assignTypeFrom">The assign type from</param>
    /// <param name="assemblies">The assemblies</param>
    /// <param name="onlyConcreteClasses">The only concrete classes</param>
    /// <returns>An enumerable of type</returns>
    protected virtual IEnumerable<Type> FindClassesOfType(
        Type assignTypeFrom,
        IEnumerable<Assembly> assemblies,
        bool onlyConcreteClasses = true
    )
    {
        List<Type> classesOfType = [];
        try
        {
            foreach (Assembly assembly in assemblies)
            {
                Type[]? typeArray = null;
                try
                {
                    typeArray = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    foreach (var le in ex.LoaderExceptions)
                    {
                        Console.WriteLine(le?.Message);
                    }
                }
                if (typeArray != null)
                {
                    foreach (Type type in typeArray)
                    {
                        if (
                            (
                                assignTypeFrom.IsAssignableFrom(type)
                                || assignTypeFrom.IsGenericTypeDefinition
                                    && DoesTypeImplementOpenGeneric(type, assignTypeFrom)
                            ) && !type.IsInterface
                        )
                        {
                            if (onlyConcreteClasses)
                            {
                                if (type.IsClass && !type.IsAbstract)
                                {
                                    classesOfType.Add(type);
                                }
                            }
                            else
                            {
                                classesOfType.Add(type);
                            }
                        }
                    }
                }
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            string message = string.Empty;
            foreach (Exception? loaderException in ex.LoaderExceptions)
            {
                message = message + loaderException?.Message + Environment.NewLine;
            }

            Exception exception = new Exception(message, ex);
            Debug.WriteLine(exception.Message, exception);
            throw exception;
        }
        return classesOfType;
    }

    /// <summary>
    /// Finds the classes of type using the specified assign type from
    /// </summary>
    /// <param name="assignTypeFrom">The assign type from</param>
    /// <param name="onlyConcreteClasses">The only concrete classes</param>
    /// <returns>An enumerable of type</returns>
    public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true)
    {
        return FindClassesOfType(assignTypeFrom, GetAssemblies(), onlyConcreteClasses);
    }

    /// <summary>
    /// Adds the assemblies in app domain using the specified added assembly names
    /// </summary>
    /// <param name="addedAssemblyNames">The added assembly names</param>
    /// <param name="assemblies">The assemblies</param>
    private void AddAssembliesInAppDomain(
        List<string> addedAssemblyNames,
        List<Assembly> assemblies
    )
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (
                !string.IsNullOrWhiteSpace(assembly.FullName)
                && Matches(assembly.FullName)
                && !addedAssemblyNames.Contains(assembly.FullName)
            )
            {
                assemblies.Add(assembly);
                addedAssemblyNames.Add(assembly.FullName);
            }
        }
    }

    /// <summary>
    /// Describes whether this instance matches
    /// </summary>
    /// <param name="assemblyFullName">The assembly full name</param>
    /// <returns>The bool</returns>
    protected virtual bool Matches(string assemblyFullName)
    {
        return !Matches(assemblyFullName, AssemblySkipLoadingPattern)
            && Matches(assemblyFullName, AssemblyRestrictToLoadingPattern);
    }

    /// <summary>
    /// Describes whether this instance matches
    /// </summary>
    /// <param name="assemblyFullName">The assembly full name</param>
    /// <param name="pattern">The pattern</param>
    /// <returns>The bool</returns>
    protected virtual bool Matches(string assemblyFullName, string pattern)
    {
        return Regex.IsMatch(
            assemblyFullName,
            pattern,
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );
    }

    /// <summary>
    /// Gets the assemblies
    /// </summary>
    /// <returns>A list of assembly</returns>
    public virtual IList<Assembly> GetAssemblies()
    {
        List<string> addedAssemblyNames = [];
        List<Assembly> assemblies = [];
        if (LoadAppDomainAssemblies)
        {
            AddAssembliesInAppDomain(addedAssemblyNames, assemblies);
        }
        AddConfiguredAssemblies(addedAssemblyNames, assemblies);
        return assemblies;
    }

    /// <summary>
    /// Adds the configured assemblies using the specified added assembly names
    /// </summary>
    /// <param name="addedAssemblyNames">The added assembly names</param>
    /// <param name="assemblies">The assemblies</param>
    protected virtual void AddConfiguredAssemblies(
        List<string> addedAssemblyNames,
        List<Assembly> assemblies
    )
    {
        foreach (string assemblyName in (IEnumerable<string>)AssemblyNames)
        {
            Assembly assembly = Assembly.Load(assemblyName);
            if (
                !string.IsNullOrWhiteSpace(assembly.FullName)
                && !addedAssemblyNames.Contains(assembly.FullName)
            )
            {
                assemblies.Add(assembly);
                addedAssemblyNames.Add(assembly.FullName);
            }
        }
    }

    /// <summary>
    /// Loads the matching assemblies using the specified directory path
    /// </summary>
    /// <param name="directoryPath">The directory path</param>
    protected virtual void LoadMatchingAssemblies(string directoryPath)
    {
        List<string> stringList = [];
        foreach (Assembly assembly in (IEnumerable<Assembly>)GetAssemblies())
        {
            if (!string.IsNullOrWhiteSpace(assembly.FullName))
                stringList.Add(assembly.FullName);
        }
        if (_fileProvider is null)
            return;

        if (!_fileProvider.DirectoryExists(directoryPath))
        {
            return;
        }

        foreach (string file in _fileProvider.GetFiles(directoryPath, "*.dll"))
        {
            try
            {
                AssemblyName assemblyName = AssemblyName.GetAssemblyName(file);
                if (Matches(assemblyName.FullName) && !stringList.Contains(assemblyName.FullName))
                {
                    App.Load(assemblyName);
                }
            }
            catch (BadImageFormatException ex)
            {
                Trace.TraceError(ex.ToString());
            }
        }
    }

    /// <summary>
    /// Describes whether this instance does type implement open generic
    /// </summary>
    /// <param name="type">The type</param>
    /// <param name="openGeneric">The open generic</param>
    /// <returns>The bool</returns>
    protected virtual bool DoesTypeImplementOpenGeneric(Type type, Type openGeneric)
    {
        try
        {
            Type genericTypeDefinition = openGeneric.GetGenericTypeDefinition();
            foreach (Type type1 in type.FindInterfaces((objType, objCriteria) => true, null))
            {
                if (
                    type1.IsGenericType
                    && genericTypeDefinition.IsAssignableFrom(type1.GetGenericTypeDefinition())
                )
                {
                    return true;
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Finds the entity type by name using the specified entity name
    /// </summary>
    /// <param name="entityName">The entity name</param>
    /// <returns>The type</returns>
    public Type? FindEntityTypeByName(string entityName)
    {
        try
        {
            var assemblies = GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                Type[]? typeArray = null;
                try
                {
                    typeArray = assembly.GetTypes();
                }
                catch
                {
                    if (!_ignoreReflectionErrors)
                    {
                        throw;
                    }
                }

                if (typeArray != null)
                {
                    var type = typeArray.FirstOrDefault(type =>
                        type.Name.Equals(entityName, StringComparison.OrdinalIgnoreCase)
                        && typeof(BaseEntity).IsAssignableFrom(type)
                        && !type.IsInterface
                        && type.IsClass
                        && !type.IsAbstract
                    );

                    if (type != null)
                    {
                        return type;
                    }
                }
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            string message = string.Empty;
            foreach (Exception? loaderException in ex.LoaderExceptions)
            {
                message = message + loaderException?.Message + Environment.NewLine;
            }

            Exception exception = new Exception(message, ex);
            Debug.WriteLine(exception.Message, exception);
            throw exception;
        }

        return null;
    }

    /// <summary>
    /// Gets the value of the app
    /// </summary>
    public virtual AppDomain App => AppDomain.CurrentDomain;

    /// <summary>
    /// Gets or sets the value of the assembly names
    /// </summary>
    public IList<string> AssemblyNames { get; set; } = [];

    /// <summary>
    /// Gets or sets the value of the load app domain assemblies
    /// </summary>
    public bool LoadAppDomainAssemblies { get; set; } = true;

    /// <summary>
    /// Gets or sets the value of the assembly skip loading pattern
    /// </summary>
    public string AssemblySkipLoadingPattern { get; set; } =
        "^System|^mscorlib|^Microsoft|^AjaxControlToolkit|^Antlr3|^Autofac|^AutoMapper|^Castle|^ComponentArt|^CppCodeProvider|^DotNetOpenAuth|^EntityFramework|^EPPlus|^FluentValidation|^ImageResizer|^itextsharp|^log4net|^MaxMind|^MbUnit|^MiniProfiler|^Mono.Math|^MvcContrib|^Newtonsoft|^NHibernate|^nunit|^Org.Mentalis|^PerlRegex|^QuickGraph|^Recaptcha|^Remotion|^RestSharp|^Rhino|^Telerik|^Iesi|^TestDriven|^TestFu|^UserAgentStringLibrary|^VJSharpCodeProvider|^WebActivator|^WebDev|^WebGrease";

    /// <summary>
    /// Gets or sets the value of the assembly restrict to loading pattern
    /// </summary>
    public string AssemblyRestrictToLoadingPattern { get; set; } = ".*";

    /// <summary>
    /// Initializes a new instance of the <see cref="AppDomainTypeFinder"/> class
    /// </summary>
    /// <param name="fileProvider">The file provider</param>
    public AppDomainTypeFinder(IO24OpenAPIFileProvider? fileProvider = null)
    {
        _fileProvider = fileProvider ?? CommonHelper.DefaultFileProvider;
    }
}
