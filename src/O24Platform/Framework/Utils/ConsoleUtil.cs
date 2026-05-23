namespace O24OpenAPI.Framework.Utils;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

/// <summary>
/// High-performance, non-blocking console logger with colors, emojis, groups and tables
/// </summary>
public static class ConsoleUtil
{
    //==============================
    //      INTERNAL MODELS
    //==============================

    /// <summary>
    /// Defines the MessageKind
    /// </summary>
    private enum MessageKind
    {
        /// <summary>
        /// Defines the Simple
        /// </summary>
        Simple,

        /// <summary>
        /// Defines the Table
        /// </summary>
        Table,

        /// <summary>
        /// Defines the GroupStart
        /// </summary>
        GroupStart,

        /// <summary>
        /// Defines the GroupEnd
        /// </summary>
        GroupEnd,
    }

    /// <summary>
    /// Defines the LogLevel
    /// </summary>
    private enum LogLevel
    {
        /// <summary>
        /// Defines the Info
        /// </summary>
        Info,

        /// <summary>
        /// Defines the Error
        /// </summary>
        Error,

        /// <summary>
        /// Defines the Success
        /// </summary>
        Success,

        /// <summary>
        /// Defines the Debug
        /// </summary>
        Debug,

        /// <summary>
        /// Defines the Warning
        /// </summary>
        Warning,

        /// <summary>
        /// Defines the Plain
        /// </summary>
        Plain,
    }

    /// <summary>
    /// Defines the <see cref="ConsoleMessage" />
    /// </summary>
    private sealed class ConsoleMessage
    {
        /// <summary>
        /// Gets the Kind
        /// </summary>
        public MessageKind Kind { get; init; }

        // For Simple / Table

        /// <summary>
        /// Gets the Level
        /// </summary>
        public LogLevel Level { get; init; } = LogLevel.Info;

        /// <summary>
        /// Gets the Template
        /// </summary>
        public string? Template { get; init; }

        /// <summary>
        /// Gets the Properties
        /// </summary>
        public (string key, object? value)[]? Properties { get; init; }

        /// <summary>
        /// Gets the Text
        /// </summary>
        public string? Text { get; init; }

        // Emoji/icon + color are derived from Level

        /// <summary>
        /// Gets the Timestamp
        /// </summary>
        public DateTime Timestamp { get; init; } = DateTime.Now;

        // Table data

        /// <summary>
        /// Gets the Title
        /// </summary>
        public string? Title { get; init; }

        /// <summary>
        /// Gets the Headers
        /// </summary>
        public IReadOnlyList<string>? Headers { get; init; }

        /// <summary>
        /// Gets the Rows
        /// </summary>
        public IReadOnlyList<IReadOnlyList<string>> Rows { get; init; }

        // Group data

        /// <summary>
        /// Gets the GroupName
        /// </summary>
        public string GroupName { get; init; }

        /// <summary>
        /// Gets a value indicating whether Collapsed
        /// </summary>
        public bool Collapsed { get; init; }

        /// <summary>
        /// The Simple
        /// </summary>
        /// <param name="level">The level<see cref="LogLevel"/></param>
        /// <param name="text">The text<see cref="string"/></param>
        /// <returns>The <see cref="ConsoleMessage"/></returns>
        public static ConsoleMessage Simple(LogLevel level, string text) =>
            new()
            {
                Kind = MessageKind.Simple,
                Level = level,
                Text = text,
            };

        /// <summary>
        /// The TemplateMessage
        /// </summary>
        /// <param name="level">The level<see cref="LogLevel"/></param>
        /// <param name="template">The template<see cref="string"/></param>
        /// <param name="properties">The properties<see cref="(string key, object? value)[]"/></param>
        /// <returns>The <see cref="ConsoleMessage"/></returns>
        public static ConsoleMessage TemplateMessage(
            LogLevel level,
            string template,
            params (string key, object? value)[] properties
        ) =>
            new()
            {
                Kind = MessageKind.Simple,
                Level = level,
                Template = template,
                Properties = properties,
            };

        /// <summary>
        /// The Table
        /// </summary>
        /// <param name="title">The title<see cref="string?"/></param>
        /// <param name="level">The level<see cref="LogLevel"/></param>
        /// <param name="headers">The headers<see cref="IReadOnlyList{string}"/></param>
        /// <param name="rows">The rows<see cref="IReadOnlyList{IReadOnlyList{string}}"/></param>
        /// <returns>The <see cref="ConsoleMessage"/></returns>
        public static ConsoleMessage Table(
            string title,
            LogLevel level,
            IReadOnlyList<string> headers,
            IReadOnlyList<IReadOnlyList<string>> rows
        ) =>
            new()
            {
                Kind = MessageKind.Table,
                Level = level,
                Title = title,
                Headers = headers,
                Rows = rows,
            };

        /// <summary>
        /// The GroupStart
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="collapsed">The collapsed<see cref="bool"/></param>
        /// <returns>The <see cref="ConsoleMessage"/></returns>
        public static ConsoleMessage GroupStart(string name, bool collapsed) =>
            new()
            {
                Kind = MessageKind.GroupStart,
                GroupName = name,
                Collapsed = collapsed,
                Level = LogLevel.Info,
            };

        /// <summary>
        /// The GroupEnd
        /// </summary>
        /// <returns>The <see cref="ConsoleMessage"/></returns>
        public static ConsoleMessage GroupEnd() => new() { Kind = MessageKind.GroupEnd };
    }

    /// <summary>
    /// Defines the <see cref="GroupContext" />
    /// </summary>
    private sealed class GroupContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupContext"/> class.
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="collapsed">The collapsed<see cref="bool"/></param>
        public GroupContext(string name, bool collapsed)
        {
            Name = name;
            Collapsed = collapsed;
            Start = DateTime.Now;
        }

        /// <summary>
        /// Gets the Name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a value indicating whether Collapsed
        /// </summary>
        public bool Collapsed { get; }

        /// <summary>
        /// Gets the Start
        /// </summary>
        public DateTime Start { get; }

        /// <summary>
        /// Gets or sets the MessageCount
        /// </summary>
        public int MessageCount { get; set; }
    }

    /// <summary>
    /// Disposable group handle for using(...) pattern
    /// </summary>
    public sealed class ConsoleLogGroup : IDisposable
    {
        /// <summary>
        /// Defines the _disposed
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogGroup"/> class.
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="collapsed">The collapsed<see cref="bool"/></param>
        internal ConsoleLogGroup(string name, bool collapsed)
        {
            Enqueue(ConsoleMessage.GroupStart(name, collapsed));
        }

        /// <summary>
        /// The Dispose
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Enqueue(ConsoleMessage.GroupEnd());
        }
    }

    //==============================
    //      QUEUE + WORKER
    //==============================

    /// <summary>
    /// Defines the _queue
    /// </summary>
    private static readonly BlockingCollection<ConsoleMessage> _queue = new();

    /// <summary>
    /// Defines the _logThread
    /// </summary>
    private static readonly Thread _logThread;

    /// <summary>
    /// Initializes static members of the <see cref="ConsoleUtil"/> class.
    /// </summary>
    static ConsoleUtil()
    {
        _logThread = new Thread(LogWorker) { IsBackground = true, Name = "ConsoleUtil-Logger" };
        _logThread.Start();
    }

    /// <summary>
    /// The LogWorker
    /// </summary>
    private static void LogWorker()
    {
        var groupStack = new Stack<GroupContext>();

        foreach (var msg in _queue.GetConsumingEnumerable())
        {
            switch (msg.Kind)
            {
                case MessageKind.GroupStart:
                    {
                        var ctx = new GroupContext(msg.GroupName ?? "Group", msg.Collapsed);
                        groupStack.Push(ctx);

                        if (!ctx.Collapsed)
                        {
                            RenderGroupHeader(ctx);
                        }
                    }
                    break;

                case MessageKind.GroupEnd:
                    {
                        if (groupStack.Count == 0)
                        {
                            break;
                        }

                        var ctx = groupStack.Pop();

                        if (ctx.Collapsed)
                        {
                            RenderCollapsedGroupSummary(ctx);
                        }
                        else
                        {
                            RenderGroupFooter(ctx);
                        }
                    }
                    break;

                default:
                    {
                        if (groupStack.Count > 0)
                        {
                            var ctx = groupStack.Peek();
                            ctx.MessageCount++;

                            if (ctx.Collapsed)
                            {
                                // Skip printing inner messages when collapsed
                                continue;
                            }
                        }

                        RenderMessage(msg);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// The Enqueue
    /// </summary>
    /// <param name="msg">The msg<see cref="ConsoleMessage"/></param>
    private static void Enqueue(ConsoleMessage msg)
    {
        if (!_queue.IsAddingCompleted)
        {
            _queue.Add(msg);
        }
    }

    //==============================
    //      PUBLIC API
    //==============================

    /// <summary>
    /// Basic non-blocking log (gray)
    /// </summary>
    /// <param name="message">The message<see cref="string"/></param>
    public static void Write(string message) =>
        Enqueue(ConsoleMessage.Simple(LogLevel.Plain, message));

    /// <summary>
    /// The WriteInfo
    /// </summary>
    /// <param name="message">The message<see cref="string"/></param>
    public static void WriteInfo(string message) =>
        Enqueue(ConsoleMessage.Simple(LogLevel.Info, message));

    /// <summary>
    /// The WriteError
    /// </summary>
    /// <param name="message">The message<see cref="string"/></param>
    public static void WriteError(string message) =>
        Enqueue(ConsoleMessage.Simple(LogLevel.Error, message));

    /// <summary>
    /// The WriteSuccess
    /// </summary>
    /// <param name="message">The message<see cref="string"/></param>
    public static void WriteSuccess(string message) =>
        Enqueue(ConsoleMessage.Simple(LogLevel.Success, message));

    /// <summary>
    /// The WriteDebug
    /// </summary>
    /// <param name="message">The message<see cref="string"/></param>
    public static void WriteDebug(string message) =>
        Enqueue(ConsoleMessage.Simple(LogLevel.Debug, message));

    /// <summary>
    /// The WriteWarning
    /// </summary>
    /// <param name="message">The message<see cref="string"/></param>
    public static void WriteWarning(string message) =>
        Enqueue(ConsoleMessage.Simple(LogLevel.Warning, message));

    // Serilog-like template API

    /// <summary>
    /// The WriteInfo
    /// </summary>
    /// <param name="template">The template<see cref="string"/></param>
    /// <param name="properties">The properties<see cref="(string key, object? value)[]"/></param>
    public static void WriteInfo(
        string template,
        params (string key, object? value)[] properties
    ) => Enqueue(ConsoleMessage.TemplateMessage(LogLevel.Info, template, properties));

    /// <summary>
    /// The WriteError
    /// </summary>
    /// <param name="template">The template<see cref="string"/></param>
    /// <param name="properties">The properties<see cref="(string key, object? value)[]"/></param>
    public static void WriteError(
        string template,
        params (string key, object? value)[] properties
    ) => Enqueue(ConsoleMessage.TemplateMessage(LogLevel.Error, template, properties));

    /// <summary>
    /// The WriteSuccess
    /// </summary>
    /// <param name="template">The template<see cref="string"/></param>
    /// <param name="properties">The properties<see cref="(string key, object? value)[]"/></param>
    public static void WriteSuccess(
        string template,
        params (string key, object? value)[] properties
    ) => Enqueue(ConsoleMessage.TemplateMessage(LogLevel.Success, template, properties));

    /// <summary>
    /// The WriteDebug
    /// </summary>
    /// <param name="template">The template<see cref="string"/></param>
    /// <param name="properties">The properties<see cref="(string key, object? value)[]"/></param>
    public static void WriteDebug(
        string template,
        params (string key, object? value)[] properties
    ) => Enqueue(ConsoleMessage.TemplateMessage(LogLevel.Debug, template, properties));

    /// <summary>
    /// The WriteWarning
    /// </summary>
    /// <param name="template">The template<see cref="string"/></param>
    /// <param name="properties">The properties<see cref="(string key, object? value)[]"/></param>
    public static void WriteWarning(
        string template,
        params (string key, object? value)[] properties
    ) => Enqueue(ConsoleMessage.TemplateMessage(LogLevel.Warning, template, properties));

    /// <summary>
    /// Create a log group. If collapsed = true, inner logs are counted but not printed,
    /// only a summary line is printed at the end
    /// </summary>
    /// <param name="name">The name<see cref="string"/></param>
    /// <param name="collapsed">The collapsed<see cref="bool"/></param>
    /// <returns>The <see cref="ConsoleLogGroup"/></returns>
    public static ConsoleLogGroup BeginGroup(string name, bool collapsed = false) =>
        new(name, collapsed);

    /// <summary>
    /// Render a nice ASCII table with headers and rows
    /// </summary>
    /// <param name="headers">The headers<see cref="IEnumerable{string}"/></param>
    /// <param name="rows">The rows<see cref="IEnumerable{IEnumerable{string}}"/></param>
    /// <param name="title">The title<see cref="string?"/></param>
    /// <param name="asInfo">The asInfo<see cref="bool"/></param>
    public static void WriteTable(
        IEnumerable<string> headers,
        IEnumerable<IEnumerable<string>> rows,
        string? title = null,
        bool asInfo = true
    )
    {
        var headersList = headers.Select(h => h ?? string.Empty).ToArray();
        var rowsList = rows.Select(r => r.Select(c => c ?? string.Empty).ToArray())
            .Cast<IReadOnlyList<string>>()
            .ToArray();

        var level = asInfo ? LogLevel.Info : LogLevel.Plain;

        Enqueue(ConsoleMessage.Table(title, level, headersList, rowsList));
    }

    //==============================
    //      RENDERING
    //==============================

    /// <summary>
    /// The RenderMessage
    /// </summary>
    /// <param name="msg">The msg<see cref="ConsoleMessage"/></param>
    private static void RenderMessage(ConsoleMessage msg)
    {
        switch (msg.Kind)
        {
            case MessageKind.Simple:
                RenderFramedSimpleMessage(msg);
                break;

            case MessageKind.Table:
                RenderTable(msg);
                break;

            default:
                // Group messages handled in worker
                break;
        }
    }

    /// <summary>
    /// The RenderFramedSimpleMessage
    /// </summary>
    /// <param name="msg">The msg<see cref="ConsoleMessage"/></param>
    private static void RenderFramedSimpleMessage(ConsoleMessage msg)
    {
        var (icon, label, color) = GetVisuals(msg.Level);
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = color;

        var text =
            msg.Text
            ?? RenderTemplate(msg.Template!, msg.Properties ?? Array.Empty<(string, object?)>());

        var border = new string('=', 50);
        Console.WriteLine(border);
        Console.WriteLine($"*  {icon} {label, -7} {msg.Timestamp:HH:mm:ss}");
        Console.WriteLine("*");

        foreach (var line in text.Split('\n'))
        {
            Console.WriteLine($"*  {line}");
        }

        Console.WriteLine("*");
        Console.WriteLine(border);
        Console.WriteLine();

        Console.ForegroundColor = oldColor;
    }

    /// <summary>
    /// The RenderTable
    /// </summary>
    /// <param name="msg">The msg<see cref="ConsoleMessage"/></param>
    private static void RenderTable(ConsoleMessage msg)
    {
        if (msg.Headers == null || msg.Rows == null)
        {
            return;
        }

        var (icon, label, color) = GetVisuals(msg.Level);
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = color;

        var border = new string('=', 50);
        Console.WriteLine(border);
        Console.WriteLine($"*  {icon} {label, -7} {msg.Timestamp:HH:mm:ss}");

        if (!string.IsNullOrWhiteSpace(msg.Title))
        {
            Console.WriteLine("*");
            Console.WriteLine($"*  {msg.Title}");
        }

        Console.WriteLine("*");

        var tableLines = BuildTableLines(msg.Headers, msg.Rows);
        foreach (var line in tableLines)
        {
            Console.WriteLine($"*  {line}");
        }

        Console.WriteLine("*");
        Console.WriteLine(border);
        Console.WriteLine();

        Console.ForegroundColor = oldColor;
    }

    /// <summary>
    /// The RenderGroupHeader
    /// </summary>
    /// <param name="ctx">The ctx<see cref="GroupContext"/></param>
    private static void RenderGroupHeader(GroupContext ctx)
    {
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;

        var border = new string('=', 50);
        Console.WriteLine(border);
        Console.WriteLine($"*  📂 GROUP  {DateTime.Now:HH:mm:ss}");
        Console.WriteLine("*");
        Console.WriteLine($"*  {ctx.Name}");
        Console.WriteLine("*");

        Console.ForegroundColor = oldColor;
    }

    /// <summary>
    /// The RenderGroupFooter
    /// </summary>
    /// <param name="ctx">The ctx<see cref="GroupContext"/></param>
    private static void RenderGroupFooter(GroupContext ctx)
    {
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;

        var border = new string('=', 50);
        var duration = DateTime.Now - ctx.Start;

        Console.WriteLine($"*");
        Console.WriteLine($"*  📁 End group '{ctx.Name}'");
        Console.WriteLine(
            $"*  Messages: {ctx.MessageCount}, Duration: {duration.TotalMilliseconds:N0} ms"
        );
        Console.WriteLine(border);
        Console.WriteLine();

        Console.ForegroundColor = oldColor;
    }

    /// <summary>
    /// The RenderCollapsedGroupSummary
    /// </summary>
    /// <param name="ctx">The ctx<see cref="GroupContext"/></param>
    private static void RenderCollapsedGroupSummary(GroupContext ctx)
    {
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;

        var border = new string('=', 50);
        var duration = DateTime.Now - ctx.Start;

        Console.WriteLine(border);
        Console.WriteLine($"*  📂 GROUP  {DateTime.Now:HH:mm:ss}");
        Console.WriteLine("*");
        Console.WriteLine($"*  {ctx.Name}");
        Console.WriteLine("*");
        Console.WriteLine($"*  (collapsed) {ctx.MessageCount} messages");
        Console.WriteLine($"*  Duration: {duration.TotalMilliseconds:N0} ms");
        Console.WriteLine(border);
        Console.WriteLine();

        Console.ForegroundColor = oldColor;
    }

    //==============================
    //      HELPERS
    //==============================

    /// <summary>
    /// The GetVisuals
    /// </summary>
    /// <param name="level">The level<see cref="LogLevel"/></param>
    /// <returns>The <see cref="(string icon, string label, ConsoleColor color)"/></returns>
    private static (string icon, string label, ConsoleColor color) GetVisuals(LogLevel level)
    {
        return level switch
        {
            LogLevel.Info => ("ℹ️", "INFO", ConsoleColor.Cyan),
            LogLevel.Error => ("❌", "ERROR", ConsoleColor.Red),
            LogLevel.Success => ("✅", "SUCCESS", ConsoleColor.Green),
            LogLevel.Debug => ("🐛", "DEBUG", ConsoleColor.DarkGray),
            LogLevel.Warning => ("⚠️", "WARN", ConsoleColor.Yellow),
            LogLevel.Plain => ("🔹", "LOG", ConsoleColor.Gray),
            _ => ("🔹", "LOG", ConsoleColor.Gray),
        };
    }

    /// <summary>
    /// The RenderTemplate
    /// </summary>
    /// <param name="template">The template<see cref="string"/></param>
    /// <param name="properties">The properties<see cref="(string key, object? value)[]"/></param>
    /// <returns>The <see cref="string"/></returns>
    private static string RenderTemplate(string template, (string key, object? value)[] properties)
    {
        if (string.IsNullOrEmpty(template) || properties.Length == 0)
        {
            return template;
        }

        var result = new StringBuilder(template);

        foreach (var (key, value) in properties)
        {
            var token = "{" + key + "}";
            var replacement = value?.ToString() ?? string.Empty;
            result.Replace(token, replacement);
        }

        return result.ToString();
    }

    /// <summary>
    /// The BuildTableLines
    /// </summary>
    /// <param name="headers">The headers<see cref="IReadOnlyList{string}"/></param>
    /// <param name="rows">The rows<see cref="IReadOnlyList{IReadOnlyList{string}}"/></param>
    /// <returns>The <see cref="IEnumerable{string}"/></returns>
    private static IEnumerable<string> BuildTableLines(
        IReadOnlyList<string> headers,
        IReadOnlyList<IReadOnlyList<string>> rows
    )
    {
        var columnCount = headers.Count;
        var widths = new int[columnCount];

        for (var i = 0; i < columnCount; i++)
        {
            widths[i] = headers[i].Length;
        }

        foreach (var row in rows)
        {
            for (var i = 0; i < columnCount && i < row.Count; i++)
            {
                var len = row[i]?.Length ?? 0;
                if (len > widths[i])
                {
                    widths[i] = len;
                }
            }
        }

        string BuildSeparator()
        {
            var sb = new StringBuilder();
            sb.Append('+');
            for (var i = 0; i < columnCount; i++)
            {
                sb.Append(new string('-', widths[i] + 2));
                sb.Append('+');
            }
            return sb.ToString();
        }

        string BuildRow(IReadOnlyList<string> cols)
        {
            var sb = new StringBuilder();
            sb.Append('|');
            for (var i = 0; i < columnCount; i++)
            {
                var val = i < cols.Count ? cols[i] ?? string.Empty : string.Empty;
                sb.Append(' ');
                sb.Append(val.PadRight(widths[i]));
                sb.Append(' ');
                sb.Append('|');
            }
            return sb.ToString();
        }

        var sep = BuildSeparator();

        yield return sep;
        yield return BuildRow(headers);
        yield return sep;

        foreach (var row in rows)
        {
            yield return BuildRow(row);
        }

        yield return sep;
    }
}
