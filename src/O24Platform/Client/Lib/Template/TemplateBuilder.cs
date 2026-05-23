using System.Text;
using Microsoft.ClearScript;
using Microsoft.ClearScript.V8;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace O24OpenAPI.Client.Lib.Template;

/// <summary>
/// The template builder class
/// </summary>
public class TemplateBuilder
{
    /// <summary>
    /// The replace
    /// </summary>
    private string __GENID = Guid.NewGuid().ToString().Replace("-", "");

    /// <summary>
    /// Gets or sets the value of the is print default meta data
    /// </summary>
    public bool IsPrintDefaultMetaData { get; set; } = true;

    /// <summary>
    /// Gets or sets the value of the is print header footer meta
    /// </summary>
    public bool IsPrintHeaderFooterMeta { get; set; } = true;

    /// <summary>
    /// Gets or sets the value of the comment text
    /// </summary>
    public string CommentText { get; set; } = "--";

    /// <summary>
    /// Gets or sets the value of the template data
    /// </summary>
    public string TemplateData { get; set; } = "";

    /// <summary>
    /// Gets or sets the value of the json parameters
    /// </summary>
    public JObject JsonParameters { get; set; } = new JObject();

    /// <summary>
    /// Gets or sets the value of the header meta
    /// </summary>
    public Dictionary<string, string> HeaderMeta { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Gets or sets the value of the footer meta
    /// </summary>
    public Dictionary<string, string> FooterMeta { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Generates the declare variables using the specified json parameters
    /// </summary>
    /// <param name="jsonParameters">The json parameters</param>
    /// <returns>The text</returns>
    private string GenerateDeclareVariables(JObject jsonParameters)
    {
        string text = "";
        foreach (JProperty item in jsonParameters.Properties())
        {
            string name = item.Name;
            JToken value = item.Value;
            text = (
                (value.Type != JTokenType.Array)
                    ? (
                        (value.Type != JTokenType.Object)
                            ? (
                                (value.Type != JTokenType.String)
                                    ? (
                                        (value.Type != JTokenType.Null)
                                            ? (text + $"let ${name} = {value};\n")
                                            : (text + "let $" + name + " = null;\n")
                                    )
                                    : (text + $"let ${name} = \"{value}\";\n")
                            )
                            : (text + $"let ${name} = {value.ToString(Formatting.None)};\n")
                    )
                    : (text + $"let ${name} = {value.ToString(Formatting.None)};\n")
            );
        }
        return text;
    }

    /// <summary>
    /// Formats the meta data using the specified dictionary
    /// </summary>
    /// <param name="dictionary">The dictionary</param>
    /// <returns>The string</returns>
    private string FormatMetaData(Dictionary<string, string> dictionary)
    {
        int num = 0;
        foreach (string key in dictionary.Keys)
        {
            if (key.Length > num)
            {
                num = key.Length;
            }
        }
        StringBuilder stringBuilder = new StringBuilder();
        foreach (KeyValuePair<string, string> item in dictionary)
        {
            StringBuilder stringBuilder2 = stringBuilder;
            StringBuilder.AppendInterpolatedStringHandler handler =
                new StringBuilder.AppendInterpolatedStringHandler(4, 3, stringBuilder2);
            handler.AppendFormatted(CommentText);
            handler.AppendLiteral(" ");
            handler.AppendFormatted(item.Key.PadLeft(num, ' '));
            handler.AppendLiteral(" : ");
            handler.AppendFormatted(item.Value);
            stringBuilder2.AppendLine(ref handler);
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// Prints the header using the specified engine
    /// </summary>
    /// <param name="engine">The engine</param>
    private void PrintHeader(V8ScriptEngine engine)
    {
        string key = "BEGIN_GEN_ID";
        string key2 = "CREATED_TIME";
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        if (IsPrintDefaultMetaData)
        {
            dictionary.Add(key, __GENID);
            dictionary.Add(
                key2,
                DateTime.Now.ToLongDateString() + "; " + DateTime.Now.ToLongTimeString()
            );
        }
        if (HeaderMeta.ContainsKey(key))
        {
            HeaderMeta.Remove(key);
        }
        if (HeaderMeta.ContainsKey(key2))
        {
            HeaderMeta.Remove(key2);
        }
        if (IsPrintHeaderFooterMeta)
        {
            foreach (KeyValuePair<string, string> headerMetum in HeaderMeta)
            {
                dictionary.Add(headerMetum.Key, headerMetum.Value);
            }
        }
        if (dictionary.Count != 0)
        {
            string[] array = FormatMetaData(dictionary).Split(Environment.NewLine);
            foreach (string text in array)
            {
                engine.Execute("console.log('" + text + "');");
            }
        }
    }

    /// <summary>
    /// Prints the footer using the specified engine
    /// </summary>
    /// <param name="engine">The engine</param>
    private void PrintFooter(V8ScriptEngine engine)
    {
        string key = "END_GEN_ID";
        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        if (IsPrintDefaultMetaData)
        {
            dictionary.Add(key, __GENID);
        }
        if (FooterMeta.ContainsKey(key))
        {
            FooterMeta.Remove(key);
        }
        if (IsPrintHeaderFooterMeta)
        {
            foreach (KeyValuePair<string, string> footerMetum in FooterMeta)
            {
                dictionary.Add(footerMetum.Key, footerMetum.Value);
            }
        }
        if (dictionary.Count != 0)
        {
            string[] array = FormatMetaData(dictionary).Split(Environment.NewLine);
            foreach (string text in array)
            {
                engine.Execute("console.log('" + text + "');");
            }
        }
    }

    /// <summary>
    /// Builds the code
    /// </summary>
    /// <returns>The string</returns>
    public string BuildCode()
    {
        using (new V8ScriptEngine())
        {
            try
            {
                string text = GenerateDeclareVariables(JsonParameters);
                return Environment.NewLine + text + Environment.NewLine + TemplateData;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
                return ex.Message;
            }
        }
    }

    /// <summary>
    /// Builds the template
    /// </summary>
    /// <returns>The string</returns>
    public string BuildTemplate()
    {
        using V8ScriptEngine v8ScriptEngine = new V8ScriptEngine();
        try
        {
            string text = TemplateData;
            string consoleOutput = string.Empty;
            v8ScriptEngine.AddHostObject("host", new HostFunctions());
            v8ScriptEngine.Script.console = new
            {
                log = (Action<object>)
                    delegate(object msg)
                    {
                        consoleOutput = consoleOutput + msg?.ToString() + Environment.NewLine;
                    },
            };
            try
            {
                PrintHeader(v8ScriptEngine);
                text = BuildCode();
                v8ScriptEngine.Execute(text);
                PrintFooter(v8ScriptEngine);
                return consoleOutput;
            }
            catch (ScriptEngineException ex)
            {
                string text2 = text;
                return text2
                    + "\nScript error: "
                    + ex.Message
                    + Environment.NewLine
                    + ex.ErrorDetails;
            }
        }
        catch (Exception ex2)
        {
            Console.WriteLine("An error occurred: " + ex2.Message);
            return ex2.Message;
        }
    }
}
