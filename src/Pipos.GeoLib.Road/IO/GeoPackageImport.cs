using System;
using System.Collections.Generic;
using System.Data.SQLite;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;
using Pipos.GeoLib.Core.Model;

namespace Pipos.GeoLib.Road.IO;
public class GeoPackageImport
{
    private string _connectionString;
    private List<Edge> Edges = new List<Edge>();
    private Dictionary<UInt64, Node> Nodes = new Dictionary<ulong, Node>();
    private List<Segment> Segments = new List<Segment>();
    private UInt32 EdgeCount;
    private UInt32 SegmentCount;
    private UInt32 NodeCount;

    public GeoPackageImport(string filePath)
    {
        _connectionString = $"Data Source={filePath};Version=3;";
    }

    public void ReadContents()
    {
        using (var connection = new SQLiteConnection(_connectionString))
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
    }


    private void ReadTable(string tableName, SQLiteConnection connection)
    {
        EdgeCount = 0;
        NodeCount = 0;
        SegmentCount = 0;
        YearSet years = new YearSet(); 
        Edges.Clear();
        Nodes.Clear();
        Segments.Clear();

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

                    Attribute attribute = new Attribute
                    {
                        Class = klass_181,
                        Ferry = Farjeled,
                        ForwardProhibited = F_ForbjudenFardriktning,
                        BackwardProhibited = B_ForbjudenFardriktning,
                        Motorway = typ_41 == 1
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

                    if (!Nodes.TryGetValue(source_pos, out sourceNode))
                    {
                        nodesExists = false;
                        sourceNode = new Node(NodeCount++);
                        Nodes.Add(source_pos, sourceNode);
                    }

                    if (!Nodes.TryGetValue(target_pos, out targetNode))
                    {
                        nodesExists = false;
                        targetNode = new Node(NodeCount++);
                        Nodes.Add(target_pos, targetNode);
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

                        int index = (i-1) * 4;
                        segments.Add(prevX);
                        segments.Add(prevY);
                        segments.Add(x);
                        segments.Add(y);
  
                        prevX = x;
                        prevY = y;
                    }

                    Edge edge = new Edge(EdgeCount, sourceNode, targetNode, extlen, f_hogst_225, b_hogst_225, segments.ToArray(), attribute, years);

                    if(nodesExists)
                    {
                        foreach(Edge e in sourceNode.Edges)
                        {
                            if(e.Equals(edge))
                            {
                                edgeExists = true;
                            }
                        }
                    }

                    if(edgeExists)
                    {
                        continue;
                    }

                    if (edge.Segments.Length == 0)
                        continue;

                    EdgeCount++;
                    SegmentCount += (UInt32)edge.Segments.Length;
                    sourceNode.Edges.Add(edge);
                    targetNode.Edges.Add(edge);
                    Edges.Add(edge);

                    for(uint s = 0; s < edge.Segments.Length; s++)
                    {
                        Segments.Add(edge.Segments[s]);
                    }
                }
            }
        }
    }

    public void ExportToBinaryFile(string path)
    {
        System.Console.WriteLine($"{NodeCount}, {EdgeCount}, {SegmentCount}");

        UInt32 headerNumber = 0x9596;
        using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
        using (BinaryWriter writer = new BinaryWriter(fs))
        {
            writer.Write(headerNumber);
            writer.Write(EdgeCount);
            writer.Write(SegmentCount);
            foreach(Edge edge in Edges)
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
    }

    private byte ReadByte(SQLiteDataReader reader, string columnName)
    {
        return reader[columnName] != DBNull.Value ? Convert.ToByte(reader[columnName]) : default(byte);
    }
}