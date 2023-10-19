using System.Diagnostics;
using System.Text;
using Npgsql;

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

    // public static void ConnectTiles(List<Node> nodes)
    // {
    //     var connections = new Dictionary<int, NVDB.Connector>();
    //     foreach (var node in nodes)
    //     {
    //         ConnectTile(node, connections);
    //     }

    //     foreach (var item in connections)
    //     {
    //         var (x, y) = ToCoordinates(item.Key);
    //         var connectionNode = new Node(x, y, NodeType.Connection);
    //         if (connectionNode.Id == item.Value.Node.Id)
    //         {
    //             //Det finns redan en punkt i vägnätet med samma koordinate
    //             continue;
    //         }
    //         var distance = (int)Math.Round(item.Value.Distance);
    //         var time = (int)Math.Round((distance / (40 / 3.6)) * 1000); 
    //         var edge = new Edge(connectionNode, item.Value.Node, distance, time);
    //         connectionNode.Edges.Add(edge);
    //         item.Value.Node.Edges.Add(edge);
    //         nodes.Add(connectionNode);
    //     }
    // }

    // static void ConnectTile(Node node, Dictionary<int, Connector> connections)
    // {
    //     var tileId = ToTileId(node);
    //     Connector? connector;
    //     if (!connections.TryGetValue(tileId, out connector))
    //     {
    //         connector = new Connector(Distance(tileId, node), node);
    //         connections[tileId] = connector;
    //         return;
    //     }
    //     var distance = Distance(tileId, node);
    //     if (distance < connector.Distance)
    //     {
    //         connector.Distance = distance;
    //         connector.Node = node;
    //     }
    // }

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