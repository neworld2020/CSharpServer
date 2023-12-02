using System.Runtime.CompilerServices;

namespace ServerLibs.LogSystem;
using System.Collections.Generic;

public class Logger
{
    public string Name { get; init; }
    public LogLevel Level { get; init; }
    private LoggerFormatter? _loggerFormatter;
    private List<LoggerAppender> _appenders;

    public Logger(string name, LogLevel level = LogLevel.Info)
    {
        Name = name;
        Level = level;
        _appenders = new List<LoggerAppender>();
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
