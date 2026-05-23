using System.Security.AccessControl;
using System.Text;
using Microsoft.Extensions.FileProviders;

namespace O24OpenAPI.Core.Infrastructure;

/// <summary>
/// The io 24 open api file provider interface
/// </summary>
/// <seealso cref="IFileProvider"/>
public interface IO24OpenAPIFileProvider : IFileProvider
{
    /// <summary>
    /// Combines the paths
    /// </summary>
    /// <param name="paths">The paths</param>
    /// <returns>The string</returns>
    string Combine(params string[] paths);

    /// <summary>
    /// Creates the directory using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    void CreateDirectory(string path);

    /// <summary>
    /// Creates the file using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    void CreateFile(string path);

    /// <summary>
    /// Deletes the directory using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    void DeleteDirectory(string path);

    /// <summary>
    /// Deletes the file using the specified file path
    /// </summary>
    /// <param name="filePath">The file path</param>
    void DeleteFile(string filePath);

    /// <summary>
    /// Describes whether this instance directory exists
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The bool</returns>
    bool DirectoryExists(string path);

    /// <summary>
    /// Directories the move using the specified source dir name
    /// </summary>
    /// <param name="sourceDirName">The source dir name</param>
    /// <param name="destDirName">The dest dir name</param>
    void DirectoryMove(string sourceDirName, string destDirName);

    /// <summary>
    /// Enumerates the files using the specified directory path
    /// </summary>
    /// <param name="directoryPath">The directory path</param>
    /// <param name="searchPattern">The search pattern</param>
    /// <param name="topDirectoryOnly">The top directory only</param>
    /// <returns>An enumerable of string</returns>
    IEnumerable<string> EnumerateFiles(
        string directoryPath,
        string searchPattern,
        bool topDirectoryOnly = true
    );

    /// <summary>
    /// Files the copy using the specified source file name
    /// </summary>
    /// <param name="sourceFileName">The source file name</param>
    /// <param name="destFileName">The dest file name</param>
    /// <param name="overwrite">The overwrite</param>
    void FileCopy(string sourceFileName, string destFileName, bool overwrite = false);

    /// <summary>
    /// Describes whether this instance file exists
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <returns>The bool</returns>
    bool FileExists(string filePath);

    /// <summary>
    /// Files the length using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The long</returns>
    long FileLength(string path);

    /// <summary>
    /// Files the move using the specified source file name
    /// </summary>
    /// <param name="sourceFileName">The source file name</param>
    /// <param name="destFileName">The dest file name</param>
    void FileMove(string sourceFileName, string destFileName);

    /// <summary>
    /// Gets the absolute path using the specified paths
    /// </summary>
    /// <param name="paths">The paths</param>
    /// <returns>The string</returns>
    string GetAbsolutePath(params string[] paths);

    /// <summary>
    /// Gets the access control using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The directory security</returns>
    DirectorySecurity GetAccessControl(string path);

    /// <summary>
    /// Gets the creation time using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The date time</returns>
    DateTime GetCreationTime(string path);

    /// <summary>
    /// Gets the directories using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <param name="searchPattern">The search pattern</param>
    /// <param name="topDirectoryOnly">The top directory only</param>
    /// <returns>The string array</returns>
    string[] GetDirectories(string path, string searchPattern = "", bool topDirectoryOnly = true);

    /// <summary>
    /// Gets the directory name using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The string</returns>
    string? GetDirectoryName(string path);

    /// <summary>
    /// Gets the directory name only using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The string</returns>
    string GetDirectoryNameOnly(string path);

    /// <summary>
    /// Gets the file extension using the specified file path
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <returns>The string</returns>
    string GetFileExtension(string filePath);

    /// <summary>
    /// Gets the file name using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The string</returns>
    string GetFileName(string path);

    /// <summary>
    /// Gets the file name without extension using the specified file path
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <returns>The string</returns>
    string GetFileNameWithoutExtension(string filePath);

    /// <summary>
    /// Gets the files using the specified directory path
    /// </summary>
    /// <param name="directoryPath">The directory path</param>
    /// <param name="searchPattern">The search pattern</param>
    /// <param name="topDirectoryOnly">The top directory only</param>
    /// <returns>The string array</returns>
    string[] GetFiles(
        string directoryPath,
        string searchPattern = "",
        bool topDirectoryOnly = true
    );

    /// <summary>
    /// Gets the last access time using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The date time</returns>
    DateTime GetLastAccessTime(string path);

    /// <summary>
    /// Gets the last write time using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The date time</returns>
    DateTime GetLastWriteTime(string path);

    /// <summary>
    /// Gets the last write time utc using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The date time</returns>
    DateTime GetLastWriteTimeUtc(string path);

    /// <summary>
    /// Gets the parent directory using the specified directory path
    /// </summary>
    /// <param name="directoryPath">The directory path</param>
    /// <returns>The string</returns>
    string? GetParentDirectory(string directoryPath);

    /// <summary>
    /// Gets the virtual path using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The string</returns>
    string? GetVirtualPath(string path);

    /// <summary>
    /// Describes whether this instance is directory
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The bool</returns>
    bool IsDirectory(string path);

    /// <summary>
    /// Maps the path using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The string</returns>
    string MapPath(string path);

    /// <summary>
    /// Reads the all bytes using the specified file path
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <returns>A task containing the byte array</returns>
    Task<byte[]> ReadAllBytes(string filePath);

    /// <summary>
    /// Reads the all text using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <param name="encoding">The encoding</param>
    /// <returns>A task containing the string</returns>
    Task<string> ReadAllTextAsync(string path, Encoding encoding);

    /// <summary>
    /// Reads the all text using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <param name="encoding">The encoding</param>
    /// <returns>The string</returns>
    string ReadAllText(string path, Encoding encoding);

    /// <summary>
    /// Writes the all bytes using the specified file path
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <param name="bytes">The bytes</param>
    Task WriteAllBytes(string filePath, byte[] bytes);

    /// <summary>
    /// Writes the all text using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <param name="contents">The contents</param>
    /// <param name="encoding">The encoding</param>
    Task WriteAllText(string path, string contents, Encoding encoding);
}
