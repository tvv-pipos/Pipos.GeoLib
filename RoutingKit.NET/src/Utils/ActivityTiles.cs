using Npgsql;

namespace Utils;

public static class ActivityTile
{
    public static async Task<List<(int x, int y)>> LoadData(string connectionString)
    {
        var sql = "select id_tile_250 from public.activity_tile";
        var result = new List<(int x, int y)>();
        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        await using (var cmd = dataSource.CreateCommand(sql))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var id = reader.GetInt64(0);
                var x = (int)(id/10000000 + 125);
                var y = (int)(id - (id/10000000 * 10000000)) + 125;
                result.Add((x, y));
            }
        }
        return result;
    }
}