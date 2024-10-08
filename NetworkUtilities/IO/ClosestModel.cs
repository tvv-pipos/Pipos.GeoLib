using System.Globalization;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using Pipos.GeoLib.NetworkUtilities.Model;
using static Pipos.GeoLib.NetworkUtilities.Model.PiposID;

namespace Pipos.GeoLib.NetworkUtilities.IO;

public class ClosestModel(
    ILogger<ClosestModel> logger,
    IConfiguration configuration
    ): IClosestModel
{
    /// <summary>
    /// Saves the result from calculation
    /// </summary>
    /// <param name="scenario"></param>
    /// <param name="transportModel"></param>
    /// <param name="storage"></param>
    /// <param name="startId"></param>
    /// <param name="result"></param>
    public async Task SaveResultAsync(Scenario scenario, string transportModel, PiposPath.Storage storage, int[] startId,
        Dictionary<string, float[]> result)
    {
        var (path, name) = PiposPath.GetClosest(scenario, transportModel, storage);
        if (storage == PiposPath.Storage.File)
            await SaveResultToFileAsync(path, name, startId, result);
        else
            await SaveResultToDatabaseAsync(path, name, startId, result);
    }

    /// <summary>
    /// Saves to a file
    /// </summary>
    /// <param name="path"></param>
    /// <param name="filename"></param>
    /// <param name="startId"></param>
    /// <param name="result"></param>
    private async Task SaveResultToFileAsync(string path, string filename, int[] startId, Dictionary<string, float[]> result)
    {
        logger.LogInformation("Saving to file, {Filename}", filename);
        
        var targetPath = Path.GetDirectoryName($"{path}/{filename}");
        if (targetPath == null)
        {
            logger.LogError("Target path is null");
            throw new IOException("Target path is null");
        }
        
        Directory.CreateDirectory(targetPath);
        var csv = new StringBuilder();
        csv.Append("id, x, y");
        foreach (var (name, res) in result)
        {
            csv.Append($", {name}");
        }

        csv.AppendLine();

        for (var i = 0; i < startId.Length; i++)
        {
            csv.Append($"{startId[i]}, {PiposID.XFromId(startId[i]) + 125}, {PiposID.YFromId(startId[i]) + 125}");
            foreach (var (name, res) in result)
            {
                csv.Append($", {res[i].ToString(CultureInfo.InvariantCulture)}");
            }

            csv.AppendLine();
        }

        await File.WriteAllTextAsync($"{path}/{filename}", csv.ToString());
        logger.LogInformation("Saved result to {Path}/{Filename}", path, filename);
    }

    /// <summary>
    /// Saves to database
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="table"></param>
    /// <param name="startId"></param>
    /// <param name="result"></param>
    private async Task SaveResultToDatabaseAsync(string schema, string table, int[] startId, Dictionary<string, float[]> result)
    {
        logger.LogInformation("Saving to database, schema {Schema}", schema);
        
        var query = $"CREATE TABLE {schema}.{table} (";
        var createTable = new StringBuilder(query, 2048);
        var columns = new StringBuilder(1024);

        foreach (var (column, value) in result)
        {
            createTable.Append($"{column} Real, ");
            columns.Append($"{column},");
        }

        createTable.Append("geom GEOMETRY, ");
        columns.Append($"geom,");

        createTable.Append("pipos_id INTEGER PRIMARY KEY)");
        columns.Append($"pipos_id");

        var copy = $"COPY {schema}.{table} ({columns}) from STDIN (FORMAT BINARY)";

        var tileWriteConnection = configuration.GetConnectionString("PiposSkrivRutData");
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(tileWriteConnection);
        dataSourceBuilder.UseNetTopologySuite();
        var datasource = dataSourceBuilder.Build();

        await using var connection = await datasource.OpenConnectionAsync();
        var cmd = new NpgsqlCommand($"CREATE SCHEMA IF NOT EXISTS {schema}", connection);
        await cmd.ExecuteNonQueryAsync();

        cmd = new NpgsqlCommand($"DROP TABLE IF EXISTS {schema}.{table}", connection);
        await cmd.ExecuteNonQueryAsync();

        cmd = new NpgsqlCommand(createTable.ToString(), connection);
        await cmd.ExecuteNonQueryAsync();

        await using var writer = await connection.BeginBinaryImportAsync(copy);
        for (var i = 0; i < startId.Length; i++)
        {
            await writer.StartRowAsync();
            foreach (var (column, value) in result)
            {
                await writer.WriteAsync(value[i]);
            }

            await writer.WriteAsync(PolygonFromId(startId[i]), NpgsqlTypes.NpgsqlDbType.Geometry);
            await writer.WriteAsync(startId[i]);
        }

        await writer.CompleteAsync();
        logger.LogInformation("Saved to database");
    }
}
