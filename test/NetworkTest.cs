using System.IO.Compression;
using NUnit.Framework;
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
    public void LoadTest()
    {
        var year = new Year(2024);
        var networkManager = new Loader()
            .FromFile(binaryPath, new YearSet(year))
            .BuildNetworkManager();

        var network = networkManager.LoadNetwork();
        var visby = network.Connect.Point(697105, 6391211, 250, year, null!);
        var ljugarn = network.Connect.Point(722621, 6361103, 250, year, null!); 
        var result = network.FindShortestDistance(visby, ljugarn, year, new QueryOptions());
        TestContext.Out.WriteLine($"The shortest distance from point A to point B is: {result.Distance} meters");
        Assert.That(result.Distance, Is.GreaterThan(40000));
    }
}