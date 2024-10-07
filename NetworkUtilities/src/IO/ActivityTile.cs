using System.Text.Json;
using Npgsql;
using Pipos.Common.NetworkUtilities.Model;

namespace Pipos.Common.NetworkUtilities.IO;
public static class ActivityTile
{
    /// <summary>
    /// Reads from DB
    /// </summary>
    /// <param name="connectionString"></param>
    /// <returns></returns>
    public static async Task<List<Node>> ReadDataFromDb(string connectionString)
    {
        var result = new List<Node>();
        await using var dataSource = NpgsqlDataSource.Create(connectionString);

        await using var cmd = dataSource.CreateCommand("SELECT id_tile_250 FROM activity_tile");
        await using var reader = await cmd.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            var tileId = reader.GetInt64(0);
            var x = (int)(tileId / 10000000);
            var y = (int)(tileId - (x * 10000000));
            result.Add(new Node(x, y, NodeType.Connection));
        }

        return result;
    }

    /// <summary>
    /// Reads 250m by 250m tiles where the population is
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="scenario"></param>
    /// <param name="extraConditions"></param>
    /// <returns></returns>
    public static async Task<List<int>> ReadPopulationTilesFromDb(string connectionString, Scenario scenario, string? extraConditions = null)
    {
        var filename = $"{Settings.PiposDataSharePath}/{scenario.ActivityTile}/ReadPopulationTilesFromDb.json";
        if (File.Exists(filename))
        {
            var json = await File.ReadAllTextAsync(filename);
            return JsonSerializer.Deserialize<List<int>>(json)!;
        }

        var result = new List<int>();
        await using var dataSource = NpgsqlDataSource.Create(connectionString);

        extraConditions = extraConditions != null ? $"AND {extraConditions}" : string.Empty;
        var query = $"""
                     SELECT pipos_id
                     FROM scenario{scenario.ActivityTile}_pd.total_all
                     WHERE (pop_male > 0 or pop_female > 0 or pop_work > 0 or synt_pop_touristbeds > 0) {extraConditions}
                     ORDER BY pipos_id
                     """;
        
        await using (var cmd = dataSource.CreateCommand(query))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var tileId = reader.GetInt32(0);
                result.Add(tileId);
            }
        }

        var jsonOut = JsonSerializer.Serialize(result);
        Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
        await File.WriteAllTextAsync(filename, jsonOut);

        return result;
    }

    /// <summary>
    /// Reads 250m by 250m activity tiles
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="scenario"></param>
    /// <returns></returns>
    public static async Task<List<int>> ReadActivityTileIdFromDb(string connectionString, Scenario scenario)
    {
        var result = new List<int>();

        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        await using var cmd = dataSource.CreateCommand($"SELECT pipos_id FROM scenario{scenario.ActivityTile}.total_all");
        await using var reader = await cmd.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            var tileId = reader.GetInt32(0);
            result.Add(tileId);
        }

        return result;
    }
}
