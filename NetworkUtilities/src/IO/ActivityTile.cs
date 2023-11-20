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

    public async static Task<List<int>> ReadPopulationTilesFromDb(string connectionString, int scenario_id)
    {
        var result = new List<int>();
        await using var dataSource = NpgsqlDataSource.Create(connectionString);

        await using (var cmd = dataSource.CreateCommand(
            $@"SELECT pipos_id FROM scenario{scenario_id}.total_all
            WHERE pop_male > 0 or pop_female > 0 or pop_work > 0 or synt_pop_touristbeds > 0"))

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

    public async static Task<List<int>> ReadActivityTileIdFromDb(string connectionString, int scenario_id)
    {
        var result = new List<int>();
        await using var dataSource = NpgsqlDataSource.Create(connectionString);

        await using (var cmd = dataSource.CreateCommand($"SELECT pipos_id FROM scenario{scenario_id}.total_all"))

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
