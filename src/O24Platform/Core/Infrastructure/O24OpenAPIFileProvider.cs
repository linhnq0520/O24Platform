using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace O24OpenAPI.Core.Infrastructure;

/// <summary>
/// /// The 24 open api file provider class
/// </summary>
/// <seealso cref="PhysicalFileProvider"/>
/// <seealso cref="IO24OpenAPIFileProvider"/>
/// <seealso cref="IFileProvider"/>
/// <remarks>
/// Initializes a new instance of the <see cref="O24OpenAPIFileProvider"/> class
/// </remarks>
/// <param name="webHostEnvironment">The web host environment</param>
public class O24OpenAPIFileProvider(IWebHostEnvironment webHostEnvironment)
    : PhysicalFileProvider(
        File.Exists(webHostEnvironment.ContentRootPath)
            ? Path.GetDirectoryName(webHostEnvironment.ContentRootPath)!
            : webHostEnvironment.ContentRootPath
    ),
        IO24OpenAPIFileProvider,
        IFileProvider
{
    /// <summary>
    /// Deletes the directory recursive using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    private static void DeleteDirectoryRecursive(string path)
    {
        Directory.Delete(path, true);
        int num = 0;
        while (Directory.Exists(path))
        {
            ++num;
            if (num > 10)
            {
                break;
            }

            Thread.Sleep(100);
        }
    }

    /// <summary>
    /// Describes whether is unc path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The bool</returns>
    protected static bool IsUncPath(string path)
    {
        Uri? result;
        return Uri.TryCreate(path, UriKind.Absolute, out result) && result.IsUnc;
    }

    /// <summary>
    /// Combines the paths
    /// </summary>
    /// <param name="paths">The paths</param>
    /// <returns>The path</returns>
    public virtual string Combine(params string[] paths)
    {
        string path = Path.Combine(
            paths
                .SelectMany<string, string>(p =>
                    !O24OpenAPIFileProvider.IsUncPath(p)
                        ? p.Split(new char[2] { '\\', '/' })
                        : (new string[1] { p })
                )
                .ToArray<string>()
        );
        if (
            Environment.OSVersion.Platform == PlatformID.Unix
            && !O24OpenAPIFileProvider.IsUncPath(path)
        )
        {
            path = "/" + path;
        }

        return path;
    }

    /// <summary>
    /// Creates the directory using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    public virtual void CreateDirectory(string path)
    {
        if (this.DirectoryExists(path))
        {
            return;
        }

        Directory.CreateDirectory(path);
    }

    /// <summary>
    /// Deletes the directory using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <exception cref="ArgumentNullException"></exception>
    public void DeleteDirectory(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentNullException(path);
        }

        foreach (string directory in Directory.GetDirectories(path))
        {
            this.DeleteDirectory(directory);
        }

        try
        {
            O24OpenAPIFileProvider.DeleteDirectoryRecursive(path);
        }
        catch (IOException ex)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(ex.Message);
            O24OpenAPIFileProvider.DeleteDirectoryRecursive(path);
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.BackgroundColor = ConsoleColor.Red;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(ex.Message);
            O24OpenAPIFileProvider.DeleteDirectoryRecursive(path);
        }
    }

    /// <summary>
    /// Deletes the file using the specified file path
    /// </summary>
    /// <param name="filePath">The file path</param>
    public void DeleteFile(string filePath)
    {
        if (!this.FileExists(filePath))
        {
            return;
        }

        File.Delete(filePath);
    }

    /// <summary>
    /// Creates the file using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    public virtual void CreateFile(string path)
    {
        if (this.FileExists(path))
        {
            return;
        }

        this.CreateDirectory(new FileInfo(path).DirectoryName);
        using (File.Create(path))
        {
            ;
        }
    }

    /// <summary>
    /// Gets the value of the web root path
    /// </summary>
    protected string WebRootPath { get; } =
        File.Exists(webHostEnvironment.WebRootPath)
            ? Path.GetDirectoryName(webHostEnvironment.WebRootPath)!
            : webHostEnvironment.WebRootPath;

    /// <summary>
    /// Describes whether this instance directory exists
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The bool</returns>
    public virtual bool DirectoryExists(string path) => Directory.Exists(path);

    /// <summary>
    /// Directories the move using the specified source dir name
    /// </summary>
    /// <param name="sourceDirName">The source dir name</param>
    /// <param name="destDirName">The dest dir name</param>
    public virtual void DirectoryMove(string sourceDirName, string destDirName)
    {
        Directory.Move(sourceDirName, destDirName);
    }

    /// <summary>
    /// Enumerates the files using the specified directory path
    /// </summary>
    /// <param name="directoryPath">The directory path</param>
    /// <param name="searchPattern">The search pattern</param>
    /// <param name="topDirectoryOnly">The top directory only</param>
    /// <returns>An enumerable of string</returns>
    public virtual IEnumerable<string> EnumerateFiles(
        string directoryPath,
        string searchPattern,
        bool topDirectoryOnly = true
    )
    {
        return Directory.EnumerateFiles(
            directoryPath,
            searchPattern,
            topDirectoryOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories
        );
    }

    /// <summary>
    /// Files the copy using the specified source file name
    /// </summary>
    /// <param name="sourceFileName">The source file name</param>
    /// <param name="destFileName">The dest file name</param>
    /// <param name="overwrite">The overwrite</param>
    public virtual void FileCopy(string sourceFileName, string destFileName, bool overwrite = false)
    {
        File.Copy(sourceFileName, destFileName, overwrite);
    }

    /// <summary>
    /// Describes whether this instance file exists
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <returns>The bool</returns>
    public virtual bool FileExists(string filePath) => File.Exists(filePath);

    /// <summary>
    /// Files the length using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The long</returns>
    public virtual long FileLength(string path)
    {
        return !this.FileExists(path) ? -1L : new FileInfo(path).Length;
    }

    /// <summary>
    /// Files the move using the specified source file name
    /// </summary>
    /// <param name="sourceFileName">The source file name</param>
    /// <param name="destFileName">The dest file name</param>
    public virtual void FileMove(string sourceFileName, string destFileName)
    {
        File.Move(sourceFileName, destFileName);
    }

    /// <summary>
    /// Gets the absolute path using the specified paths
    /// </summary>
    /// <param name="paths">The paths</param>
    /// <returns>The string</returns>
    public virtual string GetAbsolutePath(params string[] paths)
    {
        List<string> stringList = [];
        if (
            paths.Any<string>()
            && !paths[0].Contains(this.WebRootPath, StringComparison.InvariantCulture)
        )
        {
            stringList.Add(this.WebRootPath);
        }

        stringList.AddRange(paths);
        return this.Combine(stringList.ToArray());
    }

    /// <summary>
    /// Gets the access control using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The directory security</returns>
    [SupportedOSPlatform("windows")]
    public virtual DirectorySecurity GetAccessControl(string path)
    {
        return new DirectoryInfo(path).GetAccessControl();
    }

    /// <summary>
    /// Gets the creation time using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The date time</returns>
    public virtual DateTime GetCreationTime(string path) => File.GetCreationTime(path);

    /// <summary>
    /// Gets the directories using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <param name="searchPattern">The search pattern</param>
    /// <param name="topDirectoryOnly">The top directory only</param>
    /// <returns>The string array</returns>
    public virtual string[] GetDirectories(
        string path,
        string searchPattern = "",
        bool topDirectoryOnly = true
    )
    {
        if (string.IsNullOrEmpty(searchPattern))
        {
            searchPattern = "*";
        }

        return Directory.GetDirectories(
            path,
            searchPattern,
            topDirectoryOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories
        );
    }

    /// <summary>
    /// Gets the directory name using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The string</returns>
    public virtual string? GetDirectoryName(string path) => Path.GetDirectoryName(path);

    /// <summary>
    /// Gets the directory name only using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The string</returns>
    public virtual string GetDirectoryNameOnly(string path) => new DirectoryInfo(path).Name;

    /// <summary>
    /// Gets the file extension using the specified file path
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <returns>The string</returns>
    public virtual string GetFileExtension(string filePath) => Path.GetExtension(filePath);

    /// <summary>
    /// Gets the file name using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The string</returns>
    public virtual string GetFileName(string path) => Path.GetFileName(path);

    /// <summary>
    /// Gets the file name without extension using the specified file path
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <returns>The string</returns>
    public virtual string GetFileNameWithoutExtension(string filePath)
    {
        return Path.GetFileNameWithoutExtension(filePath);
    }

    /// <summary>
    /// Gets the files using the specified directory path
    /// </summary>
    /// <param name="directoryPath">The directory path</param>
    /// <param name="searchPattern">The search pattern</param>
    /// <param name="topDirectoryOnly">The top directory only</param>
    /// <returns>The string array</returns>
    public virtual string[] GetFiles(
        string directoryPath,
        string searchPattern = "",
        bool topDirectoryOnly = true
    )
    {
        if (string.IsNullOrEmpty(searchPattern))
        {
            searchPattern = "*.*";
        }

        return Directory.GetFileSystemEntries(
            directoryPath,
            searchPattern,
            new EnumerationOptions()
            {
                IgnoreInaccessible = true,
                MatchCasing = MatchCasing.CaseInsensitive,
                RecurseSubdirectories = !topDirectoryOnly,
            }
        );
    }

    /// <summary>
    /// Gets the last access time using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The date time</returns>
    public virtual DateTime GetLastAccessTime(string path) => File.GetLastAccessTime(path);

    /// <summary>
    /// Gets the last write time using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The date time</returns>
    public virtual DateTime GetLastWriteTime(string path) => File.GetLastWriteTime(path);

    /// <summary>
    /// Gets the last write time utc using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The date time</returns>
    public virtual DateTime GetLastWriteTimeUtc(string path) => File.GetLastWriteTimeUtc(path);

    /// <summary>
    /// Gets the parent directory using the specified directory path
    /// </summary>
    /// <param name="directoryPath">The directory path</param>
    /// <returns>The string</returns>
    public virtual string? GetParentDirectory(string directoryPath)
    {
        return Directory.GetParent(directoryPath)?.FullName;
    }

    /// <summary>
    /// Gets the virtual path using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The string</returns>
    public virtual string? GetVirtualPath(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }

        if (!this.IsDirectory(path) && this.FileExists(path))
        {
            path = new FileInfo(path).DirectoryName;
        }

        string? str;
        if (path == null)
        {
            str = null;
        }
        else
        {
            str = path.Replace(this.WebRootPath, string.Empty)
                .Replace('\\', '/')
                .Trim('/')
                .TrimStart('~', '/');
        }

        path = str;
        return "/" + (path ?? string.Empty);
    }

    /// <summary>
    /// Describes whether this instance is directory
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The bool</returns>
    public virtual bool IsDirectory(string path) => this.DirectoryExists(path);

    /// <summary>
    /// Maps the path using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>The string</returns>
    public virtual string MapPath(string path)
    {
        path = path.Replace("~/", string.Empty).TrimStart('/');
        string str = path.EndsWith('/') ? Path.DirectorySeparatorChar.ToString() : string.Empty;
        return this.Combine(new string[2] { this.Root ?? string.Empty, path }) + str;
    }

    /// <summary>
    /// Reads the all bytes using the specified file path
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <returns>The num array</returns>
    public virtual async Task<byte[]> ReadAllBytes(string filePath)
    {
        byte[] numArray;
        if (File.Exists(filePath))
        {
            numArray = await File.ReadAllBytesAsync(filePath);
        }
        else
        {
            numArray = Array.Empty<byte>();
        }

        return numArray;
    }

    /// <summary>
    /// Reads the all text using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <param name="encoding">The encoding</param>
    /// <returns>The str</returns>
    public virtual async Task<string> ReadAllTextAsync(string path, Encoding encoding)
    {
        StreamReader streamReader;
        await using (
            FileStream fileStream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        )
        {
            streamReader = new StreamReader(fileStream, encoding);
            try
            {
                return await streamReader.ReadToEndAsync();
            }
            finally
            {
                streamReader?.Dispose();
            }
        }
        //streamReader = (StreamReader)null;
        //string str;
        //return str;
    }

    /// <summary>
    /// Reads the all text using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <param name="encoding">The encoding</param>
    /// <returns>The string</returns>
    public virtual string ReadAllText(string path, Encoding encoding)
    {
        using (
            FileStream fileStream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        )
        {
            using (StreamReader streamReader = new(fileStream, encoding))
            {
                return streamReader.ReadToEnd();
            }
        }
    }

    /// <summary>
    /// Writes the all bytes using the specified file path
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <param name="bytes">The bytes</param>
    public virtual async Task WriteAllBytes(string filePath, byte[] bytes)
    {
        await File.WriteAllBytesAsync(filePath, bytes);
    }

    /// <summary>
    /// Writes the all text using the specified path
    /// </summary>
    /// <param name="path">The path</param>
    /// <param name="contents">The contents</param>
    /// <param name="encoding">The encoding</param>
    public virtual async Task WriteAllText(string path, string contents, Encoding encoding)
    {
        await File.WriteAllTextAsync(path, contents, encoding);
    }
}
