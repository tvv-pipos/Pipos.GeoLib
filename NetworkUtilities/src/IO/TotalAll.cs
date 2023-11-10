using Npgsql;
namespace Pipos.Common.NetworkUtilities.IO;
public static class TotalAll
{
    public static readonly string[] IgnoreColumns = { "tile_250", "pipos_id", "geom" };

    public async static Task<Dictionary<int, Dictionary<string, float>>> ReadTotalAll(string connectionString, int scenario_id)
    {
        Dictionary<int, Dictionary<string, float>> data = new Dictionary<int, Dictionary<string, float>>();

        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        await using (var cmd = dataSource.CreateCommand($"SELECT * FROM scenario{scenario_id}.total_all"))
        await using (var dataReader = cmd.ExecuteReader())
        {
            var AttributeMap = new Dictionary<string, int>(dataReader.FieldCount);
            var tmpData = new List<float>[dataReader.FieldCount];
            var piposId = new List<int>();

            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                if (!IgnoreColumns.Contains(dataReader.GetName(i)))
                {
                    AttributeMap.Add(dataReader.GetName(i), i);
                    tmpData[i] = new List<float>();
                }
            }

            while (dataReader != null && dataReader.Read())
            {
                for (int i = 0; i < dataReader.FieldCount; i++)
                {
                    if (!IgnoreColumns.Contains(dataReader.GetName(i)))
                    {
                        tmpData[i].Add(dataReader.GetFloat(i));
                    }
                    if (dataReader.GetName(i).Equals("pipos_id"))
                    {
                        piposId.Add(dataReader.GetInt32(i));
                    }
                }
            }

            for (int i = 0; i < piposId.Count; i++)
            {
                Dictionary<string, float> row = new Dictionary<string, float>();
                foreach (var (name, index) in AttributeMap)
                {
                    row.Add(name, tmpData[index][i]);
                }
                data.Add(piposId[i], row);
            }
        }
        return data;
    }

    /*public async static Task<List<int>> ReadTotalAllTiles(string connectionString, int scenario_id)
    {
        List<int> pipos_id = new List<int>();

        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        await using (var cmd = dataSource.CreateCommand($"SELECT * FROM scenario{scenario_id}.total_all"))
        await using (var dataReader = cmd.ExecuteReader())
        {
            while (dataReader != null && dataReader.Read())
            {
                for (int i = 0; i < dataReader.FieldCount; i++)
                {
                    if (dataReader.GetName(i).Equals("pipos_id"))
                    {
                        pipos_id.Add(dataReader.GetInt32(i));
                    }
                }
            }
        }
        return pipos_id;
    }*/
}
