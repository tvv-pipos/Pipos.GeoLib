using System.Text;
using Pipos.Common.NetworkUtilities.Model;

namespace Pipos.Common.NetworkUtilities.IO;

public static class Serializer
{
    public static void Serialize(this List<Node> network, string filename)
    {
        using (var stream = new FileStream(filename, FileMode.Create))
        {
            Serialize(network, stream);
        }
    }

    public static void Serialize(this List<Node> network, Stream stream)
    {
        using (var writer = new BinaryWriter(stream, new UTF8Encoding(), true))
        {
            writer.Write(network.Count());
            foreach (var node in network)
            {
                writer.Write(node.X);
                writer.Write(node.Y);
                writer.Write((int)node.NodeType);
                writer.Write(node.FunctionClass);
                writer.Write(node.NetworkGroup);
            }

            var edges = network.SelectMany(x => x.Edges).Distinct();
            writer.Write(edges.Count());
            foreach (var edge in edges)
            {
                writer.Write(edge.Source.Id);
                writer.Write(edge.Target.Id);
                writer.Write(edge.Distance);
                writer.Write(edge.ForwardSpeed);
                writer.Write(edge.BackwardSpeed);
                writer.Write(edge.ForwardTime);
                writer.Write(edge.BackwardTime);
                writer.Write(edge.IsConnectionEdge);
            }
        }
    }

    public static List<Node> DeSerialize(string filename)
    {
        using (var stream = new FileStream(filename, FileMode.Open))
        {
            return DeSerialize(stream);
        }
    }

    public static List<Node> DeSerialize(Stream stream)
    {
        using (var reader = new BinaryReader(stream, new UTF8Encoding(), true))
        {
            var nodeDict = new Dictionary<long, Node>();
            var nnodes = reader.ReadInt32();
            var idx = 0;
            for (var i = 0; i < nnodes; i++)
            {
                var node = new Node(reader.ReadInt32(), reader.ReadInt32(), (NodeType)reader.ReadInt32());
                node.FunctionClass = reader.ReadInt32();
                node.NetworkGroup = reader.ReadInt32();
                node.Index = idx++;
                nodeDict.Add(node.Id, node);
            }

            var nedges = reader.ReadInt32();
            for (var i = 0; i < nedges; i++)
            {
                var edge = new Edge(
                    source: nodeDict[reader.ReadInt64()], 
                    target: nodeDict[reader.ReadInt64()], 
                    distance: reader.ReadInt32(),
                    speedForward: reader.ReadInt32(),
                    speedBackward: reader.ReadInt32(),
                    timeForward: reader.ReadInt32(), 
                    timeBackward: reader.ReadInt32(), 
                    connectionEdge: reader.ReadBoolean());

                edge.Source.Edges.Add(edge);
                edge.Target.Edges.Add(edge);
            }
            return nodeDict.Values.ToList();
        }
    }
}