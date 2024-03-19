using System.Diagnostics;
using RoutingKit;
using Pipos.Common.NetworkUtilities.IO;

namespace UnitTest;

public class UtilsTest
{
    [SetUp]
    public void Setup()
    {
   
    }

    [OneTimeSetUp]
    public void StartTest()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());
    }

    [Test]
    public async Task Test1()
    {
        
        var graph = await Network.LoadFullNVDB(2022);
        var ch = ContractionHierarchy.Build(
            graph.NodeCount,
            graph.Tail,
            graph.Head,
            graph.GeoDistance,
            graph.Latitude,
            graph.Longitude,
            (txt) => { Debug.WriteLine(txt); }
        );

        ch.SaveToFile("/tmp/ch.bin");

        var q = new ContractionHierarchyQuery(ch);
        var result = q
            .Reset()
            .AddSource(1)
            .AddTarget(1000)
            .Run();


        Debug.WriteLine(q.GetDistance());

        double dx = graph.Latitude[0] - graph.Latitude[1000];
        double dy = graph.Longitude[0] - graph.Longitude[1000];
        Debug.WriteLine(Math.Sqrt(dx * dx + dy * dy));
        Assert.IsTrue(ch.NodeCount > 0);
    }
}