using Npgsql;

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
}