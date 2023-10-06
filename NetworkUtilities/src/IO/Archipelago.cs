using Npgsql;

public static class Archipelago
{
    public static List<long> ReadDataFromCsv(string path)
    {
        var lines = File.ReadAllLines(path);
        var length = lines.Length;
        var result = new List<long>(length);
        for (var i = 1; i < length; i++)
        {
            var tileId = Parser.ParseLong(lines[i]);
            result.Add(tileId);
        }
        return result;
    }

    public async static Task<List<long>> ReadDataFromDb(string connectionString)
    {
        var result = new List<long>();
        //var connectionString = "Host=myserver;Username=mylogin;Password=mypass;Database=mydatabase";
        await using var dataSource = NpgsqlDataSource.Create(connectionString);

        await using (var cmd = dataSource.CreateCommand("SELECT activity_tile_id_tile_250 FROM archipelago_activity_tile"))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                result.Add(reader.GetInt64(0));
            }
        }
        return result;
    }
}