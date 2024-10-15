using System.Text;
using NetTopologySuite.Algorithm;
using Pipos.GeoLib.Core.Api;
using Pipos.GeoLib.Core.Model;

namespace Pipos.GeoLib.Road;

public class Loader : ILoader
{
    private Dictionary<UInt64, Node> Nodes = new Dictionary<ulong, Node>();
    private List<Segment> Segments = new List<Segment>();
    private uint NodeId = 0;
    private int newedges = 0;
    private int oldedges = 0;

    public ILoader FromFile(string filename, YearSet years)
    {
        oldedges = newedges = 0;
        using (var stream = File.Open(filename, FileMode.Open, FileAccess.Read))
        {
            using (var reader = new BinaryReader(stream))
            {
                uint number = reader.ReadUInt32();
                if(number == 0x9596)
                {
                    uint edges_length = reader.ReadUInt32();
                    uint segments_length = reader.ReadUInt32();

                    Console.WriteLine($"{edges_length}, {segments_length}");

                    for(uint eid = 0; eid < edges_length; eid++)
                    {
                        float distance = reader.ReadSingle();
                        byte forward_speed = reader.ReadByte();
                        byte backward_speed = reader.ReadByte();
                        uint attribute = reader.ReadUInt32();
                        uint edge_seg_length = reader.ReadUInt32();
                        float[] segemnts = new float[edge_seg_length * 4];
                        for(uint s = 0; s < edge_seg_length * 4; s++)
                        {
                            segemnts[s] = reader.ReadSingle();
                        }

                        UInt64 source_pos = (UInt64)MathF.Round(segemnts[0]) | ((UInt64)MathF.Round(segemnts[1]) << 32); 
                        UInt64 target_pos = (UInt64)MathF.Round(segemnts[edge_seg_length * 4 - 2]) | ((UInt64)MathF.Round(segemnts[edge_seg_length * 4 - 1]) << 32); 

                        Node? sourceNode;
                        Node? targetNode;
                        bool nodesExists = true;
                        bool edgeExists = false;

                        if(!Nodes.TryGetValue(source_pos, out sourceNode))
                        {
                            nodesExists = false;
                            sourceNode = new Node(NodeId++);
                            Nodes.Add(source_pos, sourceNode);
                        }

                        if(!Nodes.TryGetValue(target_pos, out targetNode))
                        {
                            nodesExists = false;
                            targetNode = new Node(NodeId++);
                            Nodes.Add(target_pos, targetNode);
                        }

                        Edge edge = new Edge(eid, sourceNode, targetNode, distance, forward_speed, backward_speed, segemnts, new Attribute(attribute), years);

                        if(nodesExists)
                        {
                            foreach(Edge e in sourceNode.Edges)
                            {
                                if(e.Equals(edge))
                                {
                                    e.Years.Add(years);
                                    edgeExists = true;
                                }
                            }
                        }

                        if(edgeExists)
                        {
                            oldedges++;
                            continue;
                        }
                        newedges++;
                        sourceNode.Edges.Add(edge);
                        targetNode.Edges.Add(edge);

                        for(uint s = 0; s < edge.Segments.Length; s++)
                        {
                            Segments.Add(edge.Segments[s]);
                        }
                    }
                }
            }
        }

        Console.WriteLine($"New Edges = {newedges}, Old Edges = {oldedges}");
        return this;
    }

    public ILoader FromGeoJunkJill(Uri uri, YearSet years)
    {
        return this;
    }

    public INetworkManager BuildNetworkManager()
    {
        return new NetworkManager(new Network(new ConnectionIndexBuilder((uint)Segments.Count).AddSegments(Segments).Build()));
    }

    public void ExportToBinaryFile()
    {

    }
}
