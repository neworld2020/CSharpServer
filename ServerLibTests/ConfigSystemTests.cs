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
    }
    
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
        Console.Write(testConfigVar.ToString());
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
        var testConfigVar = new ConfigVar<TestClass>("test");
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
        var testConfigVar = new ConfigVar<TestClass>("test");
        var isSucceed = testConfigVar.FromString(testYaml);
        Assert.False(isSucceed);
        Assert.That(testConfigVar.Value, Is.Null);
    }
}