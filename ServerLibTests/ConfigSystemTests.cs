using ServerLibs.LogSystem;

namespace ServerLibTests;
using ServerLibs.ConfigSystem;

public class ConfigSystemTests
{
    private class Point
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
    private class TestClass
    {
        public int Integer { get; set; }
        public double Decimal { get; set; }
        public string String { get; set; } = "";
        public List<Point> Points { get; set; } = new List<Point>();
        public Dictionary<string, double> StringDecimalMap { get; set; } = new Dictionary<string, double>();

        public override string ToString()
        {
            return $"Int={Integer}, Decimal={Decimal}, String={String}, Points={Points}, Map={StringDecimalMap}";
        }
    }
    
    private readonly ConfigMan _configMan = ConfigMan.GetInstance();
    private readonly Logger _logger = LoggerMan.GetInstance().GetRootLogger();
    
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void SerializationTest()
    {
        var test = new TestClass
        {
            Integer = 10,
            Decimal = 3.14,
            String = "hello world",
            Points = new List<Point>() {new Point() {X = 1, Y = 2}, new Point() {X = 3, Y = 4}},
            StringDecimalMap = new Dictionary<string, double>
            {
                {"a", 1}, {"b", 2}
            }
        };
        var testConfigVar = new ConfigVar<TestClass>("test", test);
        _logger.Info(testConfigVar.ToString());
    }

    [Test]
    public void DeserializationSucceedTest()
    {
        string testYaml = @"integer: 10
decimal: 3.14
string: hello world
points:
- x: 1
  y: 2
- x: 3
  y: 4
stringDecimalMap:
  a: 1
  b: 2
";
        var testConfigVar = new ConfigVar<TestClass>("test1");
        testConfigVar.FromString(testYaml);
        Assert.That(testConfigVar.Value, Is.Not.Null);
        Assert.That(testConfigVar.Value.Integer, Is.EqualTo(10));
    }
    
    [Test]
    public void DeserializationFailedTest()
    {
        string testYaml = @"integer: 10
Decimal: 3.14
string: hello world
points:
- x: 1
  y: 2
- x: 3
  y: 4
stringDecimalMap:
  a: 1
  b: 2
";
        var testConfigVar = new ConfigVar<TestClass>("test2");
        var isSucceed = testConfigVar.FromString(testYaml);
        Assert.False(isSucceed);
    }

    [Test]
    public void ConfigFileUpdateTest()
    {
        _configMan.AddConfig<TestClass>("test", 
            "D:\\STUDY\\FUN\\CSharpServer\\ServerLibTests\\TestYAML.yml");
        var tasks = _configMan.UpdateTasks();
        Task.WaitAll(tasks.ToArray());
        // Print the config
        var testConfigVar = _configMan.GetConfig<TestClass>("test");
        _logger.Info(testConfigVar.ToString());
    }

    [Test]
    public void WrongTypeTest()
    {
        try
        {
            var wrongType = _configMan.GetConfig<Point>("test");
        }
        catch (TypeLoadException e)
        {
            Assert.Pass();
        }
        
    }
    
    [Test]
    public void UpdateReadLockTest()
    {
        // Start a Thread which read the config continuously
        for (int i = 0; i < 10; i++)
        {
            Task.Run(() =>
            {
                while (true)
                {
                    // Print the config
                    var testConfigVar = _configMan.GetConfig<TestClass>("test");
                    _logger.Info(testConfigVar.Integer.ToString());
                    Thread.Sleep(new Random().Next(10, 100));
                }
            });
        }
        Thread.Sleep(1000);
        var tasks = _configMan.UpdateTasks();
        Task.WaitAll(tasks.ToArray());
        Thread.Sleep(1000);
        
    }
}