using System.Security.Cryptography;

namespace O24OpenAPI.Client.Lib.FileConfiguration;

/// <summary>
/// The configuration file manager class
/// </summary>
public class ConfigurationFileManager
{
    /// <summary>
    /// Gets or sets the value of the   basepath
    /// </summary>
    private string __BasePath { get; set; }

    /// <summary>
    /// Gets or sets the value of the   configpath
    /// </summary>
    private string __ConfigPath { get; set; }

    /// <summary>
    /// Gets the value of the   historylogfilename
    /// </summary>
    private string __HistoryLogFileName =>
        Path.Combine(__BasePath, __ConfigPath + "/___journal.log");

    /// <summary>
    /// Gets or sets the value of the max log lines
    /// </summary>
    public int MaxLogLines { get; set; } = 5000;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationFileManager"/> class
    /// </summary>
    public ConfigurationFileManager()
        : this(AppDomain.CurrentDomain.BaseDirectory, "configuration_files") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationFileManager"/> class
    /// </summary>
    /// <param name="basePath">The base path</param>
    /// <param name="configFolderName">The config folder name</param>
    public ConfigurationFileManager(string basePath, string configFolderName)
    {
        __BasePath = basePath;
        __ConfigPath = Path.Combine(__BasePath, configFolderName);
        if (!Directory.Exists(__ConfigPath))
        {
            Directory.CreateDirectory(__ConfigPath);
        }
        if (!File.Exists(__HistoryLogFileName))
        {
            File.WriteAllText(
                __HistoryLogFileName,
                "The first time to create log file: [" + __HistoryLogFileName + "]"
            );
        }
    }

    /// <summary>
    /// Writes the history using the specified p text line
    /// </summary>
    /// <param name="pTextLine">The text line</param>
    private void __WriteHistory(string pTextLine)
    {
        string value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff") + ": " + pTextLine;
        using StreamWriter streamWriter = File.AppendText(__HistoryLogFileName);
        streamWriter.WriteLine(value);
    }

    /// <summary>
    /// Gets the changed files
    /// </summary>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <returns>The changed files</returns>
    public List<string> GetChangedFiles()
    {
        List<string> changedFiles = new List<string>();
        if (!Directory.Exists(__ConfigPath))
        {
            throw new DirectoryNotFoundException("Config directory not found: " + __ConfigPath);
        }
        Parallel.ForEach(
            (
                from w in Directory.GetFiles(__ConfigPath, "*.*", SearchOption.AllDirectories)
                where
                    !w.EndsWith(".hash")
                    && !Path.GetFileName(w).Equals(Path.GetFileName(__HistoryLogFileName))
                orderby w
                select w
            ).ToArray(),
            delegate(string configFile)
            {
                string path = configFile + ".hash";
                string text = __ComputeHash(configFile);
                bool flag = false;
                if (File.Exists(path))
                {
                    string value = File.ReadAllText(path);
                    if (!text.Equals(value))
                    {
                        flag = true;
                    }
                }
                else
                {
                    flag = true;
                    File.WriteAllText(path, "");
                }
                if (flag)
                {
                    lock (changedFiles)
                    {
                        changedFiles.Add(configFile);
                    }
                }
            }
        );
        return changedFiles;
    }

    /// <summary>
    /// Marks the file as processed successfully using the specified full file name list
    /// </summary>
    /// <param name="FullFileNameList">The full file name list</param>
    /// <param name="status">The status</param>
    public void MarkFileAsProcessedSuccessfully(List<string> FullFileNameList, string status)
    {
        __MarkFileAsProcessed(FullFileNameList, " SUCCESSFULLY - " + status);
    }

    /// <summary>
    /// Marks the file as processed unsuccessfully using the specified full file name list
    /// </summary>
    /// <param name="FullFileNameList">The full file name list</param>
    /// <param name="status">The status</param>
    public void MarkFileAsProcessedUnsuccessfully(List<string> FullFileNameList, string status)
    {
        __MarkFileAsProcessed(FullFileNameList, " BIP!BIP!BIP! UNSUCCESSFULLY - " + status);
    }

    /// <summary>
    /// Marks the file as processed using the specified full file name list
    /// </summary>
    /// <param name="FullFileNameList">The full file name list</param>
    /// <param name="status">The status</param>
    private void __MarkFileAsProcessed(List<string> FullFileNameList, string status)
    {
        List<string> list = new List<string>();
        foreach (string FullFileName in FullFileNameList)
        {
            string path = FullFileName + ".hash";
            if (File.Exists(path))
            {
                string text = __ComputeHash(FullFileName);
                string value = File.ReadAllText(path);
                if (!text.Equals(value))
                {
                    File.WriteAllText(path, text);
                    string value2 = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff");
                    string item = $"{value2}: processed [{FullFileName}].{status}";
                    list.Add(item);
                }
            }
        }
        __AppendLogsAndLimitSize(list);
    }

    /// <summary>
    /// Appends the logs and limit size using the specified logs
    /// </summary>
    /// <param name="logs">The logs</param>
    private void __AppendLogsAndLimitSize(List<string> logs)
    {
        lock (__HistoryLogFileName)
        {
            List<string> list = new List<string>();
            if (File.Exists(__HistoryLogFileName))
            {
                list.AddRange(File.ReadAllLines(__HistoryLogFileName));
            }
            list.AddRange(logs);
            if (list.Count > MaxLogLines)
            {
                list = list.GetRange(list.Count - MaxLogLines, MaxLogLines);
            }
            File.WriteAllLines(__HistoryLogFileName, list);
        }
    }

    /// <summary>
    /// Computes the hash using the specified file path
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <returns>The string</returns>
    private string __ComputeHash(string filePath)
    {
        using SHA256 sHA = SHA256.Create();
        using FileStream inputStream = File.OpenRead(filePath);
        return BitConverter
            .ToString(sHA.ComputeHash(inputStream))
            .Replace("-", "")
            .ToLowerInvariant();
    }
}
