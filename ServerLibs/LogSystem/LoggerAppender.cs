namespace ServerLibs.LogSystem;

public abstract class LoggerAppender
{
    public LogLevel Level { get; init; }
    protected LoggerAppender(LogLevel level = LogLevel.Info) => Level = level;
    
    public void Append(LogLevel level, string message)
    {
        // Ignore LOWER level message
        if(level < Level) return;
        AppendAfterCheck(message);
    }
    protected abstract void AppendAfterCheck(string message);
}

public class StdoutAppender : LoggerAppender
{
    public StdoutAppender(LogLevel level = LogLevel.Info) : base(level) { }
    
    protected override void AppendAfterCheck(string message)
    {
        Console.WriteLine(message);
    }
}

public class FileAppender(string filePath, LogLevel level = LogLevel.Info) : LoggerAppender(level)
{
    public string FilePath { get; init; } = filePath;

    protected override void AppendAfterCheck(string message)
    {
        // Write to the End
        var streamWriter = File.AppendText(FilePath);
        streamWriter.Write(message);
        streamWriter.Close();
    }
}
