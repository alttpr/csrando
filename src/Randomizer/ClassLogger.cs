using System.Diagnostics;
using Microsoft.Extensions.Logging;

internal static class ClassLogger
{
    private static volatile LogLevel _minLevel = LogLevel.Information;
    private static readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(l =>
    {
        // Dynamic filter reads current _minLevel each log call
        l.AddFilter(static (category, level) => level >= _minLevel);
        l.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
        });
    });

    public static void SetMinimumLevel(LogLevel level) => _minLevel = level;
    public static LogLevel GetMinimumLevel() => _minLevel;

    public static ILogger Get()
    {
        var stackTrace = new StackTrace(skipFrames: 1);
        var caller = stackTrace.GetFrame(0)?.GetMethod()?.DeclaringType;
        if (caller is null)
            return _loggerFactory.CreateLogger("csrando");

        return _loggerFactory.CreateLogger(caller);
    }

    public static void Shutdown() => _loggerFactory.Dispose();
}
