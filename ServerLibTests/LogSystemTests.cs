using ServerLibs.MultiThreadsSupport;

namespace ServerLibTests;
using ServerLibs.LogSystem;

public class LogSystemTests
{
    private readonly Logger _rootLogger = LoggerMan.GetInstance().GetRootLogger();
    private readonly Logger _fileLogger = new Logger("file", LoggerMan.GetInstance().GetRootLogger());
    
    [SetUp]
    public void Setup()
    {
        _fileLogger.AddAppender(new FileAppender("E:\\CSharpServer\\ServerLibTests\\Output.txt"));
    }

    [Test]
    public void RootLoggerTest()
    {
        _rootLogger.Info("Test Message");
    }

    [Test]
    public void MultiThreadStdoutTest()
    {
        int testNum = 100;
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < testNum; i++)
        {
            Task testTask = Task.Run(() =>
            {
                _rootLogger.Info("Test Task");
            });
            tasks.Add(testTask);
        }

        Task.WaitAll(tasks.ToArray());
    }
    
    [Test]
    public void MultiThreadFileTest()
    {
        int testNum = 100;
        List<Task> tasks = new List<Task>();
        for (int i = 0; i < testNum; i++)
        {
            Task testTask = Task.Run(() =>
            {
                _rootLogger.Info("File Test Task");
            });
            tasks.Add(testTask);
        }

        Task.WaitAll(tasks.ToArray());
    }
}