using System.Text.Json;
using Npgsql;
using Pipos.Common.NetworkUtilities.Model;

namespace Pipos.Common.NetworkUtilities.IO;
public static class ActivityTile
{
    public async static Task<List<Node>> ReadDataFromDb(string connectionString)
    {
        var result = new List<Node>();
        await using var dataSource = NpgsqlDataSource.Create(connectionString);

        await using (var cmd = dataSource.CreateCommand("SELECT id_tile_250 FROM activity_tile"))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var tileId = reader.GetInt64(0);
                var x = (Int32)(tileId / 10000000);
                var y = (Int32)(tileId - (x * 10000000));
                result.Add(new Node(x, y, NodeType.Connection));
            }
        }
        return result;
    }

    public async static Task<List<int>> ReadPopulationTilesFromDb(string connectionString, Scenario scenario)
    {
        string filename = $"{Settings.PiposDataSharePath}/{scenario.ActivityTile}/ReadPopulationTilesFromDb.json";
        if(File.Exists(filename))
        {
            string json = File.ReadAllText(filename);
            return JsonSerializer.Deserialize<List<int>>(json)!;
        }

        var result = new List<int>();
        await using var dataSource = NpgsqlDataSource.Create(connectionString);

        await using (var cmd = dataSource.CreateCommand(
            $@"SELECT pipos_id FROM scenario{scenario.ActivityTile}.total_all
            WHERE pop_male > 0 or pop_female > 0 or pop_work > 0 or synt_pop_touristbeds > 0 ORDER BY pipos_id"))

        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                int tileId = reader.GetInt32(0);
                result.Add(tileId);
            }
        }

        string json_out = JsonSerializer.Serialize(result);
        Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
        File.WriteAllText(filename, json_out);

        return result;
    }

    public async static Task<List<int>> ReadActivityTileIdFromDb(string connectionString, Scenario scenario)
    {
        var result = new List<int>();
        await using var dataSource = NpgsqlDataSource.Create(connectionString);

        await using (var cmd = dataSource.CreateCommand($"SELECT pipos_id FROM scenario{scenario.ActivityTile}.total_all"))

        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                int tileId = reader.GetInt32(0);
                result.Add(tileId);
            }
        }
        return result;
    }

}
