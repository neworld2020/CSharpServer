namespace ServerLibs.LogSystem;

public class LoggerMan
{
    // Logger Management: This Class uses Single-Instance structure
    private static LoggerMan? _loggerMan = null;

    private LoggerMan() {}
    public static LoggerMan GetInstance()
    {
        _loggerMan ??= new LoggerMan();
        return _loggerMan;
    }
    
    // Operations for Loggers
    private readonly List<Logger> _loggers = new List<Logger>();

    public void AddLogger(Logger logger)
    {
        _loggers.Add(logger);
    }

    public void DelLogger(string name)
    {
        var result = _loggers.Find((Logger logger) => logger.Name == name);
        if(result is null) return;
        _loggers.Remove(result);
    }

    public Logger? GetLogger(string name)
    {
        if (_loggers.Count == 0)
        {
            // Add Default Root Logger
            var rootLogger = new Logger("root", LogLevel.Info);
            rootLogger.AddAppender(new StdoutAppender());
            rootLogger.BindFormatter(new LoggerFormatter());
        }
        return _loggers.Find((Logger logger) => logger.Name == name);
    }

    public Logger GetRootLogger()
    {
        if (_loggers.Count == 0)
        {
            // Add Default Root Logger
            var rootLogger = new Logger("root", LogLevel.Info);
            rootLogger.AddAppender(new StdoutAppender());
            rootLogger.BindFormatter(new LoggerFormatter());
        }
        return _loggers[0];
    }

}