namespace O24OpenAPI.Client;

/// <summary>
/// The my console class
/// </summary>
/// <seealso cref="IDisposable"/>
public class MyConsole : IDisposable
{
    /// <summary>
    /// Writes the line using the specified p text
    /// </summary>
    /// <param name="pText">The text</param>
    public static void WriteLine(string pText)
    {
        Console.WriteLine("[" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss.fff") + "]:: " + pText);
    }

    /// <summary>
    /// Writes the line using the specified p exception
    /// </summary>
    /// <param name="pException">The exception</param>
    public static void WriteLine(Exception pException)
    {
        if (pException != null)
        {
            WriteLine(pException.ToString());
        }
    }

    /// <summary>
    /// Disposes this instance
    /// </summary>
    void IDisposable.Dispose() { }
}
