using System.Reflection;

namespace O24OpenAPI.Core.Infrastructure;

/// <summary>
/// The type finder interface
/// </summary>
public interface ITypeFinder
{
    /// <summary>
    /// Finds the classes of type using the specified only concrete classes
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="onlyConcreteClasses">The only concrete classes</param>
    /// <returns>An enumerable of type</returns>
    IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true);

    /// <summary>
    /// Finds the classes of type using the specified assign type from
    /// </summary>
    /// <param name="assignTypeFrom">The assign type from</param>
    /// <param name="onlyConcreteClasses">The only concrete classes</param>
    /// <returns>An enumerable of type</returns>
    IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true);

    /// <summary>
    /// Gets the assemblies
    /// </summary>
    /// <returns>A list of assembly</returns>
    IList<Assembly> GetAssemblies();

    /// <summary>
    /// Finds the class of type using the specified entity name
    /// </summary>
    /// <typeparam name="T">The </typeparam>
    /// <param name="entityName">The entity name</param>
    /// <returns>The type</returns>
    Type? FindClassOfType<T>(string entityName);

    /// <summary>
    /// Finds the entity type by name using the specified entity name
    /// </summary>
    /// <param name="entityName">The entity name</param>
    /// <returns>The type</returns>
    Type? FindEntityTypeByName(string entityName);
}
