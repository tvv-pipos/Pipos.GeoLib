using RoutingKit;

namespace UnitTest;

public class ContractionHierarchyTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var tail = new List<int> { 0, 1, 1};
        var head = new List<int> { 2, 3, 2};
        var weight = new List<int> { 1, 1, 1 };
        var targetList = new List<int> { 3, 2 };
        var nodeCount = 4;

        var ch = ContractionHierarchy.Build(nodeCount, tail, head, weight);
        // var q = new ContractionHierarchyQuery(ch);
        // var usedSource = q
        //     .Reset()
        //     .PinTargets(targetList)
        //     .AddSource(0)
        //     .AddSource(1)
        //     .RunToPinnedTargets();
    }
}