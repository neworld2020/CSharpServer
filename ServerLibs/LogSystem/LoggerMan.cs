namespace ServerLibs.LogSystem;

public class LoggerMan
{
    // Logger Management: This Class uses Single-Instance structure
    private static LoggerMan? _loggerMan = null;

    private LoggerMan()
    {
        // Add Default Root Logger
        var rootLogger = new Logger("root", LogLevel.Info);
        rootLogger.AddAppender(new StdoutAppender());
        rootLogger.BindFormatter(new LoggerFormatter());
        _loggers.Add(rootLogger);
    }
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
        return _loggers.Find((Logger logger) => logger.Name == name);
    }

    public Logger GetRootLogger()
    {
        return _loggers[0];
    }

}