using System.Text;

namespace ServerLibTests;
using ServerLibs.LogSystem;

public class LogSystemTests
{
    private readonly Logger _rootLogger = LoggerMan.GetInstance().GetRootLogger();
    
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void RootLoggerTest()
    {
        _rootLogger.Info("Test Message");
    }

    [Test]
    public void MakeEventTest()
    {
        _rootLogger.Info("Hello World");
    }
}