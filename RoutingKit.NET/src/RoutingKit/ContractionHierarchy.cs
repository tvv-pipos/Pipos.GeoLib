using System.Collections;
using System.Diagnostics;

namespace RoutingKit;

using CHIO = ContractionHierarchyIO;
using CHH = ContractionHierarchyHelpers;


public class ContractionHierarchy
{
    // int maxPopCount;
    // Graph graph;
    // List<int> forwardTentativeDistance;
    // List<int> backwardTentativeDistance;
    // MinIDQueue forwardQueue;
    // MinIDQueue backwardQueue;
    // TimestampFlags wasForwardPushed;
    // TimestampFlags wasBackwardPushed;

    const int DEFAULT_MAX_POP_COUNT = 500;

    int[] _rank;
    int[] _order;
    
    public int[] Rank { get => _rank; set => _rank = value; } 
    public int[] Order { get => _order; set => _order = value; }
    public Side Forward { get; set; } = new Side();
    public Side Backward { get; set; } = new Side();

    public void ResizeRank(int size)
    {
        Array.Resize(ref _rank, size);
    }

    public void ResizeOrder(int size)
    {
        Array.Resize(ref _order, size);
    }

    public class Side
    {
        public int[] FirstOut { get; set; } = null!;
        public List<int> Head { get; set; } = new List<int>();
        public List<int> Weight { get; set; } = new List<int>();
        public BitArray IsShortcutAnOriginalArc { get; set; } = null!;
        public int[] ShortcutFirstArc { get; set; } = null!;// contains input arc ID if not shortcut
        public int[] ShortcutSecondArc { get; set; } = null!;// contains input tail node ID if not shortcut
    }

    public ContractionHierarchy()
    {
        _order = null!;
        _rank = null!;
    }

    public int NodeCount => Rank.Length;

    public static ContractionHierarchy Build(
        int nodeCount, List<int> tail, List<int> head, List<int> weight, 
        Action<string>? logMessage = null, int maxPopCount = DEFAULT_MAX_POP_COUNT)
    {
        Debug.Assert(tail.Count == head.Count);
        Debug.Assert(tail.Count == weight.Count);
        Debug.Assert(tail.Max() < nodeCount);
        Debug.Assert(head.Max() < nodeCount);

        ContractionHierarchy ch = new ContractionHierarchy();
        ContractionHierarchyExtraInfo chExtra = new ContractionHierarchyExtraInfo();

        CHH.LogInputGraphStatistics(nodeCount, tail, head, logMessage);

        List<int> inputArcId = Enumerable.Range(0, head.Count).ToList();

        // Sort arcs and remove multi-arcs and loops
        CHH.SortArcsAndRemoveMultiAndLoopArcs(nodeCount, ref tail, ref head, ref weight, ref inputArcId, logMessage);

        // Build graph and order nodes
        Graph graph = new Graph(nodeCount, tail, head, weight);
        CHH.BuildChAndOrder(graph, ch, chExtra, maxPopCount, logMessage);

        // Optimize order for cache
        CHH.SortChArcsAndBuildFirstOutArrays(ch, chExtra, logMessage);
        CHH.OptimizeOrderForCache(ch, chExtra, logMessage);

        // Make internal nodes and ranks coincide
        CHH.MakeInternalNodesAndRankCoincide(ch, chExtra, logMessage);
        CHH.SortChArcsAndBuildFirstOutArrays(ch, chExtra, logMessage);

        // Build unpacking information
        CHH.BuildUnpackingInformation(nodeCount, tail, head, inputArcId, ch, chExtra, logMessage);

        LogContractionHierarchyStatistics(ch, logMessage);

        return ch;
    }
    
    public void SaveToFile(string filename)
    {
        ContractionHierarchyIO.WriteToFile(this, filename);
    }

    public static ContractionHierarchy LoadFile(string filename)
    {
        return ContractionHierarchyIO.ReadFile(filename);
    }

    static void LogContractionHierarchyStatistics(ContractionHierarchy ch, Action<string>? logMessage)
    {
        if (logMessage != null)
        {
            logMessage($"CH has {ch.Forward.Head.Count} forward arcs.");
            logMessage($"CH has {ch.Backward.Head.Count} backward arcs.");
        }
    }


    // Implement the missing functions here or use suitable C# equivalents.

}