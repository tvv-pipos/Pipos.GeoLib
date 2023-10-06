using System.Diagnostics;
using NUnit.Framework.Internal;
using RoutingKit;
using Utils;

namespace UnitTest;

[TestFixture]
public class ContractionQueryTest
{
    ContractionHierarchy _ch = null!;
    RoutingGraph _graph = null!;

    [SetUp]
    public void Setup()
    {
        
    }

    [OneTimeSetUp]
    public void StartTest()
    {
        _ch = ContractionHierarchy.LoadFile("/tmp/ch.bin");
        _graph = Network.ReadFromFile("/tmp/graph.bin");
        
        Trace.Listeners.Add(new ConsoleTraceListener());
    }

    [Test]
    public void TestOneToOne()
    {
        var query = new ContractionHierarchyQuery(_ch);
        var nodes = new int[] { 1, 100, 1000, 10000, 100000, 1000000, 10000000 };

        foreach (var source in nodes)
        {
            foreach (var target in nodes)
            {
                if (source != target)
                {
                    query
                        .Reset()
                        .AddSource(source)
                        .AddTarget(target)
                        .Run();
                    double dx = _graph.Latitude[source] - _graph.Latitude[target];
                    double dy = _graph.Longitude[source] - _graph.Longitude[target];
                    var graphDistance = query.GetDistance();
                    var euclideanDistance = Math.Round(Math.Sqrt(dx * dx + dy * dy));
                    var ratio = Math.Round(graphDistance / euclideanDistance, 3);
                    Assert.Less(euclideanDistance, graphDistance);
                    Assert.Less(ratio, 2);
                    Debug.WriteLine($"Graph distance: {graphDistance}, Euclidean distance: {euclideanDistance}, Ratio: {ratio}");
                }
            }
        }
        Assert.IsTrue(true);
    }

    [Test]
    public void TestOneToMany()
    {
        var query = new ContractionHierarchyQuery(_ch);
        var nodes = new List<int> { 1, 100, 1000, 10000, 100000, 1000000, 10000000 };

        query.Reset().PinTargets(nodes);
        foreach (var source in nodes)
        {
            var result = query
                .ResetSource()
                .AddSource(source)
                .RunToPinnedTargets()
                .GetDistancesToTargets();

            for (var i = 0; i < nodes.Count; i++)
            {
                var target = nodes[i];
                if (target == source)
                {
                    continue;
                }

                double dx = _graph.Latitude[source] - _graph.Latitude[target];
                double dy = _graph.Longitude[source] - _graph.Longitude[target];
                var graphDistance = result[i];
                var euclideanDistance = Math.Round(Math.Sqrt(dx * dx + dy * dy));
                var ratio = Math.Round(graphDistance / euclideanDistance, 3);
                Assert.Less(euclideanDistance, graphDistance);
                Assert.Less(ratio, 2);
                Debug.WriteLine($"Graph distance: {graphDistance}, Euclidean distance: {euclideanDistance}, Ratio: {ratio}");
            }
        }
    }

    [Test]
    public void TestManyToOne()
    {
        var query = new ContractionHierarchyQuery(_ch);
        var nodes = new List<int> { 1, 100, 1000, 10000, 100000, 1000000, 10000000 };

        query.Reset().PinSources(nodes);
        foreach (var target in nodes)
        {
            var result = query
                .ResetTarget()
                .AddTarget(target)
                .RunToPinnedSources()
                .GetDistancesToSources();

            for (var i = 0; i < nodes.Count; i++)
            {
                var source = nodes[i];
                if (target == source)
                {
                    continue;
                }

                double dx = _graph.Latitude[source] - _graph.Latitude[target];
                double dy = _graph.Longitude[source] - _graph.Longitude[target];
                var graphDistance = result[i];
                var euclideanDistance = Math.Round(Math.Sqrt(dx * dx + dy * dy));
                var ratio = Math.Round(graphDistance / euclideanDistance, 3);
                Assert.Less(euclideanDistance, graphDistance);
                Assert.Less(ratio, 2);
                Debug.WriteLine($"Graph distance: {graphDistance}, Euclidean distance: {euclideanDistance}, Ratio: {ratio}");
            }
        }
    }
}