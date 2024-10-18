using System.IO.Compression;
using NUnit.Framework;
using Pipos.GeoLib.Core.Api;
using Pipos.GeoLib.Core.Model;
using Pipos.GeoLib.Road;

[TestFixture]
public class NetworkTest
{
    string binaryPath = string.Empty;

    [SetUp]
    public void SetUp()
    {
        var zipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "../../../../data/Tillganglighetsvagnat_240101_gotland.zip");
        var tempDir = Path.GetTempPath();
        var extractedFilePath = Path.Combine(tempDir, "Tillganglighetsvagnat_240101_gotland.bin");
        
        if (!Directory.Exists(tempDir))
        {
            Directory.CreateDirectory(tempDir);
        }

        if (!File.Exists(extractedFilePath))
        {
            ZipFile.ExtractToDirectory(zipPath, tempDir);
        }
        
        binaryPath = extractedFilePath;
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(binaryPath))
        {
            File.Delete(binaryPath);
        }
    }

    [Test]
    public void FindShortestDistance_Test()
    {
        var year = new Year(2024);
        var network = new Loader()
            .FromFile(binaryPath, new YearSet(year))
            .BuildNetworkManager()
            .LoadNetwork();

        var visby = network.Connect.Point(697105, 6391211, 250, year, null!);
        var ljugarn = network.Connect.Point(722621, 6361103, 250, year, null!); 
        Assert.That(visby.IsConnected(), Is.True);
        Assert.That(ljugarn.IsConnected(), Is.True);

        var result = network.FindShortestDistance(visby, ljugarn, year, new QueryOptions());
        Assert.That(result.HasResult, Is.True);

        TestContext.Out.WriteLine($"The shortest distance from point A to point B is: {result.Distance} meters");
        Assert.That(result.Distance, Is.GreaterThan(40000));        
    }

    [Test]
    public void FindWithinTime_Test()
    {
        var year = new Year(2024);
        var network = new Loader()
            .FromFile(binaryPath, new YearSet(year))
            .BuildNetworkManager()
            .LoadNetwork();

        var visby = network.Connect.Point(697105, 6391211, 250, year, null!);
        Assert.That(visby.IsConnected(), Is.True);

        var connections = new List<IConnection>();
        for (var x = 697105; x < 722621; x += 10000)
        {
            for (var y = 6361103; y < 6391211; y += 10000)
            {
                var connection = network.Connect.Point(x, y, 5000, year, null!);
                Assert.That(connection.IsConnected, Is.True);
                connections.Add(connection);
            }
        }

        var result = network.FindWithinTime(visby, 180, year, null!);
        Assert.That(result.HasResult, Is.True);
        
        var totalDistances = 0;
        foreach (var connection in connections)
        {
            var distance = result.FindTime(connection, new QueryOptions());
            if (distance.HasResult && distance.Time > 0)
            {
                totalDistances++;
            }
        }
        TestContext.Out.WriteLine($"Total: {connections.Count} results: {totalDistances}");
    }
}