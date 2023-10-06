using RoutingKit;
using Utils;

//BuildGraph().Wait();
BuildAndSave();
//LoadAndQuery();
//TestKDIndex().Wait();


static async Task BuildGraph()
{
     var graph = await Network.LoadNVDB();
     Network.WriteToFile(graph, "/tmp/graph.bin");
}

static void BuildAndSave()
{
     long timer = RoutingKit.Timer.GetMicroTime();
     var graph = Network.ReadFromFile("/tmp/graph.bin");
     var ch = ContractionHierarchy.Build(
         graph.NodeCount,
         graph.Tail,
         graph.Head,
         graph.GeoDistance
         ,txt => { Console.WriteLine(txt); }
     );
     ch.SaveToFile("/tmp/ch.bin");
     System.Console.WriteLine("Time = " + (RoutingKit.Timer.GetMicroTime() - timer));
}

static void LoadAndQuery()
{
    var ch = ContractionHierarchy.LoadFile("/tmp/ch.bin");
    var graph = Network.ReadFromFile("/tmp/graph.bin");

    var q = new ContractionHierarchyQuery(ch);
    var result = q
        .Reset()
        .AddSource(1)
        .AddTarget(1000)
        .Run();
    double dx = graph.Latitude[1] - graph.Latitude[1000];
    double dy = graph.Longitude[1] - graph.Longitude[1000];
    Console.WriteLine(Math.Sqrt(dx * dx + dy * dy));
    Console.WriteLine(q.GetDistance());
}

static async Task TestKDIndex()
{
    var graph = Network.ReadFromFile("/tmp/graph.bin");
    var connectionString = "Server=pipos.dev.tillvaxtverket.se;database=pipos_master;user id=REMOVED_SECRET;password=REMOVED_SECRET;port=40000";
    var coordinates = await ActivityTile.LoadData(connectionString);

    var index = new KDIndex(graph.Latitude.Count);
    for (var i = 0; i < graph.Latitude.Count; i++)
    {
        index.Add(graph.Longitude[i], graph.Latitude[i]);
    }
    index.Finish();

    var sw = System.Diagnostics.Stopwatch.StartNew();
    var counter = new bool[coordinates.Count];
    var result = new int[coordinates.Count];

    Parallel.For(0, coordinates.Count, i => 
    {
        var x = coordinates[i].x;
        var y = coordinates[i].y;
        var radius = 250;
        var neighbours = index.Within(x, y, radius);
        while (!neighbours.Any())
        {
            if (radius > 5000)
            {
                break;
            }
            radius += 250;
            neighbours = index.Within(x, y, radius);
        }

        if (neighbours.Any())
        {
            result[i] = neighbours.OrderBy(idx => SquareDist(graph.Longitude[idx], graph.Latitude[idx], x, y)).First();
            counter[i] = true;
        }
    });
    var elapsed = sw.Elapsed;
    Console.WriteLine($"Elapsed time {elapsed}, connected {counter.Count(x => x)} of {coordinates.Count}");
}

static int SquareDist(int x1, int y1, int x2, int y2) 
{
    var dx = x1 - x2;
    var dy = y1 - y2;
    return dx * dx + dy * dy;
}