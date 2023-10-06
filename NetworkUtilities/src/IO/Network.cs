using RoutingKit;
using System.Linq;
using Pipos.Common.NetworkUtilities.Model;
using Pipos.Common.NetworkUtilities.Processing;

namespace Pipos.Common.NetworkUtilities.IO;

public static class Network
{
    public static async Task<RoutingGraph> LoadNVDB()
    {
        var connectionString = "Server=pipos.dev.tillvaxtverket.se;database=pipos_master;user id=REMOVED_SECRET;password=REMOVED_SECRET;port=40000";
        var connectionStringrut = "Server=pipos.dev.tillvaxtverket.se;database=pip_rutdata;user id=REMOVED_SECRET;password=REMOVED_SECRET;port=40000";

        var nodes = await NVDB.ReadData(connectionString, 2022);


        var population_tiles = await ActivityTile.ReadPopulationTilesFromDb(connectionStringrut, 2022);
        // Pin nodes with population
        //GraphOptimizer.PinActivityTile(nodes, population_tiles);
        //GraphOptimizer.Optimize(nodes);
        
        HashSet<Edge> edges_set = new HashSet<Edge>();

        var graph = new RoutingGraph();
        var nnodes = nodes.Count;
        graph.Latitude = new List<int>(new int[nnodes]);
        graph.Longitude = new List<int>(new int[nnodes]);

        var nodeIdx = 0;    
        foreach (var node in nodes)
        {
            node.Idx = nodeIdx;
            graph.Latitude[nodeIdx] = node.Y;
            graph.Longitude[nodeIdx] = node.X;
            nodeIdx++;
            foreach(var edge in node.Edges)
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
                graph.TravelTime.Add((int)Math.Round((edge.Distance / (float)edge.ForwardTime) * 3600));
            }

            if (edge.BackwardTime > 0)
            {
                graph.Tail.Add(edge.Target.Idx);
                graph.Head.Add(edge.Source.Idx);
                graph.GeoDistance.Add(edge.Distance);
                graph.TravelTime.Add((int)Math.Round((edge.Distance / (float)edge.BackwardTime) * 3600));
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
}