using System.Diagnostics;
using Microsoft.Extensions.Logging;

internal static class ClassLogger
{
    private static readonly ILoggerFactory _loggerFactory;
    static ClassLogger()
    {
        _loggerFactory = LoggerFactory.Create(l => l.AddSimpleConsole(options =>
        {
            options.IncludeScopes = true;
            options.SingleLine = true;
        }));
    }

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
