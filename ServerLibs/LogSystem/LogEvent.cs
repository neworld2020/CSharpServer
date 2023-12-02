namespace ServerLibs.LogSystem;
using ServerLibs.Utils;

internal record LogEvent
{
    // Log Appending File
    public required string FileName { get; init; }
    // Log Appending Line Number
    public required uint LineNum { get; init; }
    // Log Appending Time
    public required DateTimeOffset LogTime { get; init; }
    // Elapse Time from Starting the Program
    public required TimeSpan ElapseTime { get; init; }
    // Thread Information
    public required uint ThreadId { get; init; }
    public required string ThreadName { get; init; }
    // Fabric Information
    public required uint FabricId { get; init; }
    // Log Message
    public required string Message { get; init; } = "";
    // Logger Class Name: Modified by Logger
    public string LoggerClass { get; set; } = "";
    
    public static LogEvent Make(string message,
                                 string? fileName = null, 
                                 uint? lineNum = null,
                                 DateTimeOffset? logTime = null,
                                 TimeSpan? elapseTime = null,
                                 uint? threadId = null,
                                 string? threadName = null,
                                 uint? fabricId = null)
    {
        return new LogEvent
        {
            ElapseTime = elapseTime ??= TimeSpan.Zero,
            FileName = fileName ??= Utils.GetFileName(4),
            LineNum = lineNum ??= Utils.GetLineNum(4),
            LogTime = logTime ??= DateTimeOffset.Now,
            ThreadId = threadId ??= 0,
            ThreadName = threadName ??= "main",
            FabricId = fabricId ??= 0,
            Message = message
        };
    }
};
