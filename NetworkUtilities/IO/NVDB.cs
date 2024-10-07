using System.Diagnostics;
using System.Text;
using Npgsql;
using Pipos.GeoLib.NetworkUtilities.Model;

namespace Pipos.GeoLib.NetworkUtilities.IO;

public static class NVDB
{
    public async static Task<List<Node>> ReadData(string connectionString, Scenario scenario)
    {
        string sql = String.Empty;

        /* TODO: use scenario db */
        if(scenario.NVDB == 2014)
            sql = @"SELECT ST_AsText(geom) AS WKT, id, b_hogst_225 as b_kkod, f_hogst_225 as f_kkod, klass_181 as function_class, networkgrp as network_group FROM nvdb_2014_ver2";
        else
            sql = @"SELECT ST_AsText(geom) AS WKT, id, b_hogst_225 as b_kkod, f_hogst_225 as f_kkod, klass_181 as function_class, networkgrp as network_group FROM nvdb_2022_ver3";

        return await ReadData(connectionString, sql, scenario);
    }
    private async static Task<List<Node>> ReadData(string connectionString, string sql, Scenario scenario)
    {
        Console.WriteLine($"Read and build graph...");
        var sw = Stopwatch.StartNew();
        var result = new Dictionary<long, Node>();
        
        await using var dataSource = NpgsqlDataSource.Create(connectionString);

        await using (var cmd = dataSource.CreateCommand(sql))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var idx = 0;
                var wkt = reader.GetString(idx++);
                var id = reader.GetInt64(idx++);
                var backwardSpeed = 0;
                var forwardSpeed = 0;
                var functionClass = 0;

                if(!reader.IsDBNull(idx++))
                {
                    //backwardSpeed = reader.GetInt32(idx - 1);
                    backwardSpeed = int.Parse(reader.GetString(idx - 1));
                }

                if(!reader.IsDBNull(idx++))
                {
                    //forwardSpeed = reader.GetInt32(idx - 1);
                    forwardSpeed = int.Parse(reader.GetString(idx - 1));
                }

                if(!reader.IsDBNull(idx++))
                {
                    //functionClass = reader.GetInt32(idx - 1);
                    functionClass = int.Parse(reader.GetString(idx - 1));
                }

                var networkGroup = reader.GetInt32(idx++);

                var lineString = Parser.ParseLineString(wkt);
                var nodes = Parser.ParseNodes(lineString, forwardSpeed, backwardSpeed, networkGroup, functionClass);
                if (nodes.Count > 1)
                {
                    MergeNodes(result, nodes);
                }
            }
        }
        Console.WriteLine($"Read and build done ({sw.Elapsed})");
        var data = result.Values.ToList();
        return data;
    }

    public class Connector
    {
        public Connector(double distance, Node node)
        {
            Distance = distance;
            Node = node;
        }

        public double Distance { get; set; }
        public Node Node { get; set; }
    }

    static (int x, int y) ToCoordinates(int tileId)
    {
        var x = tileId & 0xFFFF;
        var y = (tileId >> 16);
        return ((x * 250) + 125, (y * 250) + 125);
    }

    static double Distance(int tileId, Node node)
    {
        var (x, y) = ToCoordinates(tileId);

        double dx = x - node.X;
        double dy = y - node.Y;
        return Math.Sqrt(dx * dx + dy * dy);
    }

    private static void MergeNodes(Dictionary<long, Node> result, List<Node> nodes)
    {
        foreach (var node in nodes)
        {
            if (result.TryGetValue(node.Id, out Node? existingNode))
            {
                foreach (var edge in node.Edges)
                {
                    existingNode.FunctionClass = Math.Min(existingNode.FunctionClass, node.FunctionClass);
                    edge.ReplaceNode(node, existingNode);
                    existingNode.Edges.Add(edge);
                }
                node.Edges.Clear();
            }
            else
            {
                result[node.Id] = node;
            }
        }
    }

    static List<string> Split(string line, char separator)
    {
        var result = new List<string>();
        var inQuotes = false;
        var currentStr = new StringBuilder();
        for (var i = 0; i < line.Length; i++)
        {
            if (line[i] == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (line[i] == separator)
            {
                if (!inQuotes)
                {
                    result.Add(currentStr.ToString());
                    currentStr.Clear();
                }
                else
                {
                    currentStr.Append(line[i]);
                }
            }
            else
            {
                currentStr.Append(line[i]);
            }
        }
        result.Add(currentStr.ToString());
        return result;
    }

}