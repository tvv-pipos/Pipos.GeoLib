using RoutingKit;
using Pipos.Common.NetworkUtilities.Model;
using Pipos.Common.NetworkUtilities.Processing;
using System.Diagnostics;

namespace Pipos.Common.NetworkUtilities.IO;

public static class Network
{
    public static async Task<RoutingGraph> LoadFullNVDB(int scenario_id)
    {
        /* TODO: Use env variables */
        var connectionString = "Server=pipos.dev.tillvaxtverket.se;database=pipos_master;user id=REMOVED_SECRET;password=REMOVED_SECRET;port=40000";
        // var connectionStringrut = "Server=pipos.dev.tillvaxtverket.se;database=pip_rutdata;user id=REMOVED_SECRET;password=REMOVED_SECRET;port=40000";

        List<Node> nodes = await NVDB.ReadData(connectionString, scenario_id);

        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].Idx = i;
        }

        BackwardRemove(nodes);
        ForwardRemove(nodes);
        int nodeCount = 0;

        foreach (var node in nodes)
        {
            if (node.Idx >= 0)
            { nodeCount++; }
        }

        Console.WriteLine($"Removed {nodes.Count - nodeCount} nodes from network!");

        HashSet<Edge> edges_set = new HashSet<Edge>();

        var graph = new RoutingGraph();
        var nnodes = nodeCount;
        graph.Latitude = new List<int>(new int[nnodes]);
        graph.Longitude = new List<int>(new int[nnodes]);

        var nodeIdx = 0;    
        foreach (var node in nodes)
        {
            if (node.Idx >= 0)
            {
                node.Idx = nodeIdx;
                graph.Latitude[nodeIdx] = node.Y;
                graph.Longitude[nodeIdx] = node.X;
                nodeIdx++;
                foreach (var edge in node.Edges)
                    edges_set.Add(edge);
            }
        }

        var edges = edges_set.ToList();
        for (var i = 0; i < edges.Count; i++) 
        {
            var edge = edges[i];
            if (edge.ForwardTime > 0 && edge.Source.Idx >= 0 && edge.Target.Idx >= 0) // kontrollera enkelriktat
            {
                graph.Tail.Add(edge.Source.Idx);
                graph.Head.Add(edge.Target.Idx);
                graph.GeoDistance.Add(edge.Distance);
                graph.TravelTime.Add(edge.ForwardTime);
            }

            if (edge.BackwardTime > 0 && edge.Source.Idx >= 0 && edge.Target.Idx >= 0)
            {
                graph.Tail.Add(edge.Target.Idx);
                graph.Head.Add(edge.Source.Idx);
                graph.GeoDistance.Add(edge.Distance);
                graph.TravelTime.Add(edge.BackwardTime);
            }
        }

        var inputArcId = Sort.ComputeSortPermutationUsingLess(graph.Tail);
        graph.Tail = Permutation.ApplyPermutation(inputArcId, graph.Tail);
        graph.Head = Permutation.ApplyPermutation(inputArcId, graph.Head);
        graph.GeoDistance = Permutation.ApplyPermutation(inputArcId, graph.GeoDistance);
        graph.TravelTime = Permutation.ApplyPermutation(inputArcId, graph.TravelTime);
        graph.FirstOut = InvVecUtils.InvertVector(graph.Tail, graph.Latitude.Count);

        return graph;
    }

    public static async Task<RoutingGraph> LoadOptimizedNVDB(int scenario_id)
    {
        var connectionString = "Server=pipos.dev.tillvaxtverket.se;database=pipos_master;user id=REMOVED_SECRET;password=REMOVED_SECRET;port=40000";
        var connectionStringrut = "Server=pipos.dev.tillvaxtverket.se;database=pip_rutdata;user id=REMOVED_SECRET;password=REMOVED_SECRET;port=40000";

        List<Node> nodes = await NVDB.ReadData(connectionString, scenario_id);

        var activity_tiles = await ActivityTile.ReadPopulationTilesFromDb(connectionStringrut, scenario_id);
        activity_tiles.AddRange(await TravelReason.ReadTravelReasonTiles(connectionStringrut, scenario_id));

        // Removed unconnected nodes 
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].Idx = i;
        }

        BackwardRemove(nodes);
        ForwardRemove(nodes);
        int nodeCount = nodes.Count;

        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            if (nodes[i].Idx < 0)
            {
                foreach (var edge in nodes[i].Edges)
                {
                    var other = edge.GetOtherNode(nodes[i]);
                    if (other != nodes[i])
                    {
                        for (int e = other.Edges.Count - 1; e >= 0; e--)
                        {
                            if (other.Edges[e].Source == nodes[i] || other.Edges[e].Target == nodes[i])
                            {
                                other.Edges.Remove(other.Edges[e]);
                            }
                        }
                    }
                    else
                        Debug.Assert(false);
                }
                nodes.Remove(nodes[i]);
            }
        }

        Console.WriteLine($"Removed {nodeCount - nodes.Count} nodes from network!");

        // Pin nodes with population
        GraphOptimizer.PinActivityTile(nodes, activity_tiles);
        GraphOptimizer.OptimizeNetwork(nodes);

        HashSet<Edge> edges_set = new HashSet<Edge>();

        var graph = new RoutingGraph();
        var nnodes = nodes.Count();
        graph.Latitude = new List<int>(new int[nnodes]);
        graph.Longitude = new List<int>(new int[nnodes]);

        var nodeIdx = 0;
        foreach (var node in nodes)
        {
            node.Idx = nodeIdx;
            graph.Latitude[nodeIdx] = node.Y;
            graph.Longitude[nodeIdx] = node.X;
            nodeIdx++;
            foreach (var edge in node.Edges)
                edges_set.Add(edge);
        }

        var edges = edges_set.ToList();
        for (var i = 0; i < edges.Count; i++)
        {
            var edge = edges[i];
            if (edge.ForwardTime > 0) // kontrollera enkelriktat
            {
                graph.Tail.Add(edge.Source.Idx);
                graph.Head.Add(edge.Target.Idx);
                graph.GeoDistance.Add(edge.Distance);
                graph.TravelTime.Add(edge.ForwardTime);
            }

            if (edge.BackwardTime > 0)
            {
                graph.Tail.Add(edge.Target.Idx);
                graph.Head.Add(edge.Source.Idx);
                graph.GeoDistance.Add(edge.Distance);
                graph.TravelTime.Add(edge.BackwardTime);
            }
        }

        var inputArcId = Sort.ComputeSortPermutationUsingLess(graph.Tail);
        graph.Tail = Permutation.ApplyPermutation(inputArcId, graph.Tail);
        graph.Head = Permutation.ApplyPermutation(inputArcId, graph.Head);
        graph.GeoDistance = Permutation.ApplyPermutation(inputArcId, graph.GeoDistance);
        graph.TravelTime = Permutation.ApplyPermutation(inputArcId, graph.TravelTime);
        graph.FirstOut = InvVecUtils.InvertVector(graph.Tail, graph.Latitude.Count);

        return graph;
    }

    public static void WriteToFile(RoutingGraph graph, string filename)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
        using (var fileStream = new FileStream(filename, FileMode.Create))
        using (var writer = new BinaryWriter(fileStream))
        {
            writer.Write(graph.Tail.Count);
            ContractionHierarchyIO.WriteVector(writer, graph.Tail);

            writer.Write(graph.Head.Count);
            ContractionHierarchyIO.WriteVector(writer, graph.Head);

            writer.Write(graph.GeoDistance.Count);
            ContractionHierarchyIO.WriteVector(writer, graph.GeoDistance);

            writer.Write(graph.TravelTime.Count);
            ContractionHierarchyIO.WriteVector(writer, graph.TravelTime);

            writer.Write(graph.FirstOut.Length);
            ContractionHierarchyIO.WriteVector(writer, graph.FirstOut);

            writer.Write(graph.Latitude.Count);
            ContractionHierarchyIO.WriteVector(writer, graph.Latitude);

            writer.Write(graph.Longitude.Count);
            ContractionHierarchyIO.WriteVector(writer, graph.Longitude);
        }
    }

    public static RoutingGraph ReadFromFile(string filename)
    {
        using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
        using (BinaryReader reader = new BinaryReader(stream))
        {
            var graph = new RoutingGraph();
            var tailCount = reader.ReadInt32();
            graph.Tail = ContractionHierarchyIO.ReadVector(reader, tailCount);

            var headCount = reader.ReadInt32();
            graph.Head = ContractionHierarchyIO.ReadVector(reader, headCount);

            var geoDistanceCount = reader.ReadInt32();
            graph.GeoDistance = ContractionHierarchyIO.ReadVector(reader, geoDistanceCount);

            var travelTimeCount = reader.ReadInt32();
            graph.TravelTime = ContractionHierarchyIO.ReadVector(reader, travelTimeCount);

            var firstOutCount = reader.ReadInt32();
            graph.FirstOut = ContractionHierarchyIO.ReadArray<int>(reader, firstOutCount);
            
            var latitudeCount = reader.ReadInt32();
            graph.Latitude = ContractionHierarchyIO.ReadVector(reader, latitudeCount);

            var longitudeCount = reader.ReadInt32();
            graph.Longitude = ContractionHierarchyIO.ReadVector(reader, longitudeCount);
            
            return graph;
        }
    }

    private static void BackwardRemove(List<Node> nodes)
    {
        int[] time = new int[nodes.Count];
        Array.Fill(time, Int32.MaxValue, 0, time.Length);

        MinHeap queue = new MinHeap();
        int index = 0;
        for(int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].Edges.Count >= 4)
            {
                index = i;
                break;
            }
        }

        queue.Add((uint)index, 0);
        time[index] = 0;

        while (!queue.IsEmpty)
        {
            index = (int)queue.RemoveMin();

            foreach (var edge in nodes[index].Edges)
            {
                var next = edge.GetOtherNode(nodes[index]);
                if (next.Idx >= 0 && ((edge.Target == next && edge.BackwardTime > 0) || (edge.Source == next && edge.ForwardTime > 0)))
                {
                    int length = time[index] + edge.Distance;
                    if (time[next.Idx] > length)
                    {
                        time[next.Idx] = length;
                        queue.Add((uint)next.Idx, (uint)length);
                    }
                }
            }
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            if (time[i] == Int32.MaxValue)
                nodes[i].Idx = -1;
        }
    }

    private static void ForwardRemove(List<Node> nodes)
    {
        int[] time = new int[nodes.Count];
        Array.Fill(time, Int32.MaxValue, 0, time.Length);

        MinHeap queue = new MinHeap();
        int index = 0;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].Edges.Count >= 4)
            {
                index = i;
                break;
            }
        }

        queue.Add((uint)index, 0);
        time[index] = 0;

        while (!queue.IsEmpty)
        {
            index = (int)queue.RemoveMin();

            foreach (var edge in nodes[index].Edges)
            {
                var next = edge.GetOtherNode(nodes[index]);
                if (next.Idx >= 0 && ((edge.Target == next && edge.ForwardTime > 0) || (edge.Source == next && edge.BackwardTime > 0)))
                {
                    int length = time[index] + edge.Distance;
                    if (time[next.Idx] > length)
                    {
                        time[next.Idx] = length;
                        queue.Add((uint)next.Idx, (uint)length);
                    }
                }
            }
        }

        for (int i = 0; i < nodes.Count; i++)
        {
            if (time[i] == Int32.MaxValue)
                nodes[i].Idx = -1;
        }
    }

}