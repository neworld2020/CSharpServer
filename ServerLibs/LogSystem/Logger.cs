using System.Runtime.CompilerServices;

namespace ServerLibs.LogSystem;
using System.Collections.Generic;

public class Logger
{
    public string Name { get; init; }
    public LogLevel Level { get; init; }
    private LoggerFormatter? _loggerFormatter;
    private List<LoggerAppender> _appenders;

    /// <summary>
    /// Create a new Logger with a name and a level
    /// </summary>
    /// <param name="name">The name of the new logger</param>
    /// <param name="level">The level of the logger, lower level of messages will be ignored.</param>
    public Logger(string name, LogLevel level = LogLevel.Info)
    {
        Name = name;
        Level = level;
        _appenders = new List<LoggerAppender>();
        // Add myself to LoggerMan
        var loggerMan = LoggerMan.GetInstance();
        loggerMan.AddLogger(this);
    }

    /// <summary>
    /// Create a new logger with the same configuration as another logger
    /// </summary>
    /// <param name="name"> The name of the new Logger </param>
    /// <param name="other"> The logger whose configurations you want to copy</param>
    /// <example>
    /// <code>
    /// // Create a new logger with the same configuration as the root logger
    /// var newLogger = new Logger("newLogger", LoggerMan.GetInstance().GetRootLogger());
    /// </code>
    /// </example>
    public Logger(string name, Logger other)
    {
        // Copy Configuration of Another Logger (Usually Root)
        Name = name;
        Level = other.Level;
        _appenders = other._appenders;
        _loggerFormatter = other._loggerFormatter;
        // Add myself to LoggerMan
        var loggerMan = LoggerMan.GetInstance();
        loggerMan.AddLogger(this);
    }

    ~Logger()
    {
        // Delete myself in LoggerMan
        var loggerMan = LoggerMan.GetInstance();
        loggerMan.DelLogger(this.Name);
    }
    
    // Formatter Operations
    public void BindFormatter(LoggerFormatter formatter)
    {
        _loggerFormatter = formatter;
    }
    
    // Appenders Operation
    public void AddAppender(LoggerAppender appender)
    {
        _appenders.Add(appender);
    }

    public void RemoveAppender(LoggerAppender appender)
    {
        _appenders.Remove(appender);
    }
    
    public void Log(LogLevel level, string message)
    {
        // Ignore message LOWER than logger inherent level
        if (level < Level)
        {
            return;
        }

        var e = LogEvent.Make(message);
        // Logger -> LoggerFormatter -> LoggerAppender
        e.LoggerClass = Name;
        if(_loggerFormatter is null) return;
        string formatStr = _loggerFormatter.Format(level, e);
        foreach (var appender in _appenders)
        {
            appender.Append(level, formatStr);
        }
    }
    
    public void Debug(string message) {Log(LogLevel.Debug, message);}
    public void Info(string message) {Log(LogLevel.Info, message);}
    public void Warn(string message) {Log(LogLevel.Warn, message);}
    public void Error(string message) {Log(LogLevel.Error, message);}
    public void Fatal(string message) {Log(LogLevel.Fatal, message);}

}
