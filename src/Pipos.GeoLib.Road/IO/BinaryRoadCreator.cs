using System;
using System.Collections.Generic;
using System.Data.SQLite;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;
using Pipos.GeoLib.Core.Model;

namespace Pipos.GeoLib.Road.IO;
public class BinaryRoadCreator
{
    private List<Edge> Edges = new List<Edge>();
    private int SegmentCount = 0;

    public BinaryRoadCreator Clear()
    {
        Edges.Clear();
        SegmentCount = 0;

        return this;
    }

    public BinaryRoadCreator ImportGeoPackage(string filePath)
    {
        string connectionString = $"Data Source={filePath};Version=3;";

        using (var connection = new SQLiteConnection(connectionString))
        {
            string tableName = "";
            connection.Open();
            string tableQuery = "SELECT table_name, srs_id FROM gpkg_contents";

            using (var command = new SQLiteCommand(tableQuery, connection))
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    var name = reader["table_name"].ToString();
                    if (!string.IsNullOrEmpty(name))
                        tableName = name;
                }
            }

            if (tableName != "")
                ReadTable(tableName, connection);
        }
        return this;
    }

    public BinaryRoadCreator ExportToBinaryFile(string path)
    {
        UInt32 headerNumber = 0x9596;
        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
        using (BinaryWriter writer = new BinaryWriter(fs))
        {
            writer.Write(headerNumber);
            writer.Write(Edges.Count);
            writer.Write(SegmentCount);
            foreach (Edge edge in Edges)
            {
                writer.Write(edge.Distance);
                writer.Write(edge.ForwardSpeed);
                writer.Write(edge.BackwardSpeed);
                writer.Write(edge.Attribute.Value);
                writer.Write(edge.Segments.Length);
                foreach(Segment segment in edge.Segments)
                {
                    writer.Write(segment.X1);
                    writer.Write(segment.Y1);
                    writer.Write(segment.X2);
                    writer.Write(segment.Y2);
                }
            }
        }
        return this;     
    }

    public BinaryRoadCreator SetFerrySpeed(byte speed)
    {
        foreach (Edge edge in Edges)
        {
            if(edge.Attribute.Ferry)
            {
                if(!edge.Attribute.ForwardProhibited)
                    edge.ForwardSpeed = speed;

                if(!edge.Attribute.BackwardProhibited)              
                    edge.BackwardSpeed = speed;
            }
        }
        return this;
    }

    public BinaryRoadCreator Validate()
    {
        foreach (Edge edge in Edges)
        {
            if(edge.ForwardSpeed == 0 && !edge.Attribute.ForwardProhibited)
            {
                throw new Exception("edge.ForwardSpeed == 0 && !edge.Attribute.ForwardProhibited");
            }

            if(edge.BackwardSpeed == 0 && !edge.Attribute.BackwardProhibited)
            {
                throw new Exception("edge.BackwardSpeed == 0 && !edge.Attribute.BackwardProhibited");
            }

            if(edge.Attribute.ForwardProhibited && edge.Attribute.BackwardProhibited)
            {
                throw new Exception("edge.Attribute.ForwardProhibited && edge.Attribute.BackwardProhibited");
            }
        }
        return this;
    }

    public BinaryRoadCreator CalculateDisconnectedIslands()
    {
        Queue<Edge> q = new Queue<Edge>();
        bool[] visited = new bool[Edges.Count];
        int[] group = new int[Edges.Count];
        int group_id = 0;
        int maxCount = 0;
        int maxCountGroup = 0;

        foreach(Edge edge in Edges)
        {
            int edgeCount = 0;
            if(visited[edge.Id] == false)
            {
                visited[edge.Id] = true;
                edgeCount++;

                q.Enqueue(edge);

                while (q.TryDequeue(out var current)) 
                {
                    group[current.Id] = group_id;

                    if(!current.Attribute.ForwardProhibited)
                    {
                        foreach(Edge neighbor in current.Target.Edges)
                        {
                            if (!visited[neighbor.Id]) 
                            {
                                visited[neighbor.Id] = true;
                                edgeCount++;
                                q.Enqueue(neighbor);
                            }
                        }
                    }

                    if(!current.Attribute.BackwardProhibited)
                    {
                        foreach(Edge neighbor in current.Source.Edges)
                        {
                            if (!visited[neighbor.Id]) 
                            {
                                visited[neighbor.Id] = true;
                                edgeCount++;
                                q.Enqueue(neighbor);
                            }
                        }
                    }
                }
                if(edgeCount > maxCount) 
                {
                    maxCount = edgeCount;
                    maxCountGroup = group_id;
                }
                group_id++;
            }
        }

        // Recalculate ids
        foreach (Edge edge in Edges)
        {
            if(group[edge.Id] != maxCountGroup) 
            {
                var attr = edge.Attribute;
                attr.DisconnectedIsland = true;
                edge.Attribute = attr;
            }
        }

        return this;
    }

    public BinaryRoadCreator Optimize()
    {
        HashSet<Node> nodes = new HashSet<Node>();

        Edges.ForEach(e => 
        {
            nodes.Add(e.Source);
            nodes.Add(e.Target);
        });

        int count = 1;
        while (count > 0)
        {
            count = 0;
            foreach (var node in nodes)
            {
                if (node.Edges.Count == 2)
                {
                    Edge e1 = node.Edges[0];
                    Edge e2 = node.Edges[1];
                    if(e1.Id == UInt32.MaxValue || e2.Id == UInt32.MaxValue)
                        continue;

                    if (e1.ForwardSpeed == e2.ForwardSpeed && e1.BackwardSpeed == e2.BackwardSpeed && e1.Attribute.Value == e2.Attribute.Value)
                    {
                        if (e1.Target == node && e2.Source == node)
                        {
                            e1.AddSegmentsAfter(e2);
                            e1.Distance += e2.Distance;
                            e2.Target.ReplaceEdge(e2, e1);
                            e1.Target = e2.Target;
                            e2.Id = UInt32.MaxValue;
                            node.Id = UInt32.MaxValue;
                            count++;
                        }
                        else if (e1.Source == node && e2.Target == node)
                        {
                            e2.AddSegmentsAfter(e1);
                            e2.Distance += e1.Distance;
                            e1.Target.ReplaceEdge(e1, e2);
                            e2.Target = e1.Target;
                            e1.Id = UInt32.MaxValue;
                            node.Id = UInt32.MaxValue;
                            count++;
                        }
                    }
                    else if (e1.ForwardSpeed == e2.BackwardSpeed && e1.BackwardSpeed == e2.ForwardSpeed && e1.Attribute.Value == e2.Attribute.Reverse().Value)
                    {
                        if (e1.Target == node && e2.Target == node)
                        {
                            e1.AddSegmentsAfterReveresed(e2);
                            e1.Distance += e2.Distance;
                            e2.Source.ReplaceEdge(e2, e1);
                            e1.Target = e2.Source;
                            e2.Id = UInt32.MaxValue;
                            node.Id = UInt32.MaxValue;
                            count++;
                        }
                        else if (e1.Source == node && e2.Source == node)
                        {
                            e2.AddSegmentsBeforeReversed(e1);
                            e2.Distance += e1.Distance;
                            e1.Target.ReplaceEdge(e1, e2);
                            e2.Source = e1.Target;
                            e1.Id = UInt32.MaxValue;
                            node.Id = UInt32.MaxValue;
                            count++;
                        }
                    }
                }
            }
        }
        Edges = Edges.Where(e => e.Id != UInt32.MaxValue).ToList();

        for (int i = 0; i < Edges.Count; i++)
        {
            Edges[i].Id = (uint)i;
        }

        return this;
    }
  
    private byte ReadByte(SQLiteDataReader reader, string columnName)
    {
        return reader[columnName] != DBNull.Value ? Convert.ToByte(reader[columnName]) : default(byte);
    }

    private void ReadTable(string tableName, SQLiteConnection connection)
    {
        Dictionary<UInt64, Node> nodes = new Dictionary<ulong, Node>();
        uint nodeCount = 0;
        uint edgeCount = 0;
        YearSet years = new YearSet(); 
        Clear();

        var geoReader = new GeoPackageGeoReader();
        string selectNetowrk = $"SELECT geom, b_hogst_225, f_hogst_225, klass_181, extlen, F_ForbjudenFardriktning, B_ForbjudenFardriktning, Farjeled, typ_41 FROM {tableName};";
        using (var command = new SQLiteCommand(selectNetowrk, connection))
        {
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    byte[] wkb = (byte[])reader["geom"];
                    Geometry geometry = geoReader.Read(wkb);

                    Coordinate[] coordinates;

                    if (geometry is LineString lineString)
                    {
                        coordinates = lineString.Coordinates;
                        if (coordinates.Length < 2)
                            continue;
                    }
                    else
                    {
                        continue;
                    }

                    byte b_hogst_225 = ReadByte(reader, "b_hogst_225");
                    byte f_hogst_225 = ReadByte(reader, "f_hogst_225");
                    byte klass_181 = ReadByte(reader, "klass_181");
                    float extlen = Convert.ToSingle(reader["extlen"]);
                    bool F_ForbjudenFardriktning = Convert.ToBoolean(reader["F_ForbjudenFardriktning"]);
                    bool B_ForbjudenFardriktning = Convert.ToBoolean(reader["B_ForbjudenFardriktning"]);
                    bool Farjeled = Convert.ToBoolean(reader["Farjeled"]);
                    byte typ_41 = ReadByte(reader, "typ_41");

                    if(F_ForbjudenFardriktning && B_ForbjudenFardriktning)
                    {
                        continue;
                    }

                    Attribute attribute = new Attribute
                    {
                        Class = klass_181,
                        Ferry = Farjeled,
                        ForwardProhibited = F_ForbjudenFardriktning,
                        BackwardProhibited = B_ForbjudenFardriktning,
                        Motorway = typ_41 == 1,
                        DisconnectedIsland = false
                    };

                    float prevX = (float)coordinates[0].X;
                    float prevY = (float)coordinates[0].Y;
                    float lastX = (float)coordinates[coordinates.Length - 1].X;
                    float lastY = (float)coordinates[coordinates.Length - 1].Y;

                    UInt64 source_pos = (UInt64)MathF.Round(prevX) | ((UInt64)MathF.Round(prevY) << 32);
                    UInt64 target_pos = (UInt64)MathF.Round(lastX) | ((UInt64)MathF.Round(lastY) << 32);

                    Node? sourceNode;
                    Node? targetNode;
                    bool nodesExists = true;
                    bool edgeExists = false;

                    if (!nodes.TryGetValue(source_pos, out sourceNode))
                    {
                        nodesExists = false;
                        sourceNode = new Node(nodeCount++);
                        nodes.Add(source_pos, sourceNode);
                    }

                    if (!nodes.TryGetValue(target_pos, out targetNode))
                    {
                        nodesExists = false;
                        targetNode = new Node(nodeCount++);
                        nodes.Add(target_pos, targetNode);
                    }

                    List<float> segments = new List<float>((coordinates.Length - 1) * 4);

                    for (int i = 1; i < coordinates.Length; i++)
                    {
                        float x = (float)coordinates[i].X;
                        float y = (float)coordinates[i].Y;
                        if (x == prevX && y == prevY)
                        {
                            continue;
                        }

                        int index = (i - 1) * 4;
                        segments.Add(prevX);
                        segments.Add(prevY);
                        segments.Add(x);
                        segments.Add(y);
  
                        prevX = x;
                        prevY = y;
                    }

                    Edge edge = new Edge(edgeCount, sourceNode, targetNode, extlen, f_hogst_225, b_hogst_225, segments.ToArray(), attribute, years);

                    if (nodesExists)
                    {
                        foreach (Edge e in sourceNode.Edges)
                        {
                            if (e.IsSame(edge))
                            {
                                edgeExists = true;
                            }
                        }
                    }

                    if (edgeExists)
                    {
                        continue;
                    }

                    if (edge.Segments.Length == 0)
                        continue;

                    edgeCount++;
                    SegmentCount += edge.Segments.Length;
                    sourceNode.Edges.Add(edge);
                    targetNode.Edges.Add(edge);
                    Edges.Add(edge);
                }
            }
        }
    }
}