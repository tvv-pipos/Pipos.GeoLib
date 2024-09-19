using System.Diagnostics;
using System.Text;
using Npgsql;
using Pipos.Common.NetworkUtilities.Model;

namespace Pipos.Common.NetworkUtilities.IO;

public static class NVDB
{
    public async static Task<List<Node>> ReadDataGotland(string connectionString)
    {
        var sql = @"
            SELECT ST_AsText(the_geom) AS WKT, id, b_kkod, f_kkod, function_class, network_group 
            FROM road_segment
            WHERE ST_Intersects(the_geom, ST_GeomFromText('POLYGON ((670158.616364627 6443498.32614359,771465.213997485 6444662.76979455,764769.663004509 6301145.08981466,671031.949102841 6301145.08981466,670158.616364627 6443498.32614359))', 3006))";
        return await ReadData(connectionString, sql);
    }

    public async static Task<List<Node>> ReadData(string connectionString, int scenario_id)
    {
        /* TODO: use scenario db */
        var sql = @"
            SELECT ST_AsText(the_geom) AS WKT, id, b_kkod, f_kkod, function_class, network_group 
            FROM road_segment";

        return await ReadData(connectionString, sql);
    }
    private async static Task<List<Node>> ReadData(string connectionString, string sql)
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
                var backwardSpeed = reader.GetInt32(idx++);
                var forwardSpeed = reader.GetInt32(idx++);
                var functionClass = reader.GetInt32(idx++);
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
        return result.Values.ToList();
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


    public async static Task<List<Node>> LoadNetwork(string connectionString, int scenario_id)
    {
        List<Node> nodes = new List<Node>();
        await LoadRoadData(nodes, connectionString, scenario_id);
        LineSweep(nodes);
        return nodes;
    }

    private async static Task LoadRoadData(List<Node> nodes, string connectionString, int scenario_id)
    {
        var sql = @"
            SELECT ST_AsText(the_geom) AS WKT, id, b_kkod, f_kkod, function_class, network_group 
            FROM road_segment";

        Console.WriteLine($"Read and build graph...");
        var sw = Stopwatch.StartNew();
        var result = new Dictionary<long, Node>();

        await using var dataSource = NpgsqlDataSource.Create(connectionString);

        await using (var cmd = dataSource.CreateCommand(sql))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                Node? last_point = null;
                int x = 0, y = 0, x2 = 0, y2 = 0;

                var idx = 0;
                var wkt = reader.GetString(idx++);
                var id = reader.GetInt64(idx++);
                var backwardSpeed = reader.GetInt32(idx++);
                var forwardSpeed = reader.GetInt32(idx++);
                var functionClass = reader.GetInt32(idx++);
                var networkGroup = reader.GetInt32(idx++);

                // TODO: Only connect to right group
                if (networkGroup == 0)
                {
                    var lineString = Parser.ParseLineString(wkt);
                    for(int i = 0; i < lineString.Length; i++)
                    {
                        x = lineString[i][0];
                        y = lineString[i][1];

                        Node point = new Node {
                            Idx = nodes.Count(),
                            X = x,
                            Y = y,
                            FunctionClass = functionClass,
                            NetworkGroup = networkGroup
                        };

                        if (last_point != null)
                        {
                            if (last_point.Id == point.Id)
                            {
                                continue;
                            }

                            int dx = x2 - x;
                            int dy = y2 - y;

                            int distance = (int)Math.Round(Math.Sqrt(dx * dx + dy * dy));

                            Edge edge = new Edge
                            {
                                Source = last_point,
                                Target = point,
                                Distance = distance,
                                ForwardSpeed = forwardSpeed,
                                BackwardSpeed = backwardSpeed,
                                ForwardTime = forwardSpeed == 0 ? 0 : (int)Math.Round((float)distance / (float)forwardSpeed * 3600.0f),
                                BackwardTime = backwardSpeed == 0 ? 0 : (int)Math.Round((float)distance / (float)backwardSpeed * 3600.0f)
                            };

                            last_point.Edges.Add(edge);
                            point.Edges.Add(edge);
                        }
                        nodes.Add(point);
                        last_point = point;
                        x2 = x;
                        y2 = y;
                    }
                }
            }
        }
        Console.WriteLine($"Read and build done ({sw.Elapsed})");
    }

    private static void LineSweep(List<Node> nodes)
    {
        List<Node> connected_nodes = new List<Node>();
        nodes.Sort(delegate (Node n1, Node n2)
        {
            if (n1.Y == n2.Y)
                return n1.X.CompareTo(n2.X);
            return n1.Y.CompareTo(n2.Y);
        });

        Node? last_node = null;

        foreach (Node node in nodes)
        {
            if (last_node != null
                && node.Y == last_node.Y
                && node.X == last_node.X)
            {
                foreach (Edge edge in node.Edges)
                {
                    last_node.Edges.Add(edge);
                    if (edge.Source.Idx == node.Idx)
                        edge.Source = last_node;
                    else if (edge.Target.Idx == node.Idx)
                        edge.Target = last_node;
                    else
                        Debug.Assert(false);
                }
                node.Edges.Clear();
            }
            else
            {
                connected_nodes.Add(node);
                last_node = node;
            }
        }
        //var result = nodes.Where(n => n.Edges.Count > 0).ToArray();
        nodes.Clear();
        nodes.AddRange(connected_nodes.ToArray());
    }
}