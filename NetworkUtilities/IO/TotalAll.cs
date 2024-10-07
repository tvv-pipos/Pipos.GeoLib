using System.Text.Json;
using Npgsql;
using Pipos.GeoLib.NetworkUtilities.Model;

namespace Pipos.GeoLib.NetworkUtilities.IO;
public static class TotalAll
{
    public static readonly string[] IgnoreColumns = { "tile_250", "pipos_id", "geom"};
    public static readonly string[] CarAvailabilityColumns = {"antal_fordon_i_trafik", "pop_20_64", "pop_65_79"};

    public async static Task<DataMatrix> ReadTotalAll(string connectionString, Scenario scenario)
    {
        string filename = $"{Settings.PiposDataSharePath}/{scenario.ActivityTile}_{scenario.CarAvailability}/TotalAll.json";
        if(File.Exists(filename))
        {
            string json = File.ReadAllText(filename);
            return new DataMatrix(JsonSerializer.Deserialize<Dictionary<int, Dictionary<string, float>>>(json)!);
        }
        
        Dictionary<int, Dictionary<string, float>> data = new Dictionary<int, Dictionary<string, float>>();

        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        await using (var cmd = dataSource.CreateCommand($"SELECT * FROM scenario{scenario.ActivityTile}.total_all"))
        await using (var dataReader = cmd.ExecuteReader())
        {
            var AttributeMap = new Dictionary<string, int>(dataReader.FieldCount);
            var tmpData = new List<float>[dataReader.FieldCount];
            var piposId = new List<int>();

            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                if (!IgnoreColumns.Contains(dataReader.GetName(i)) && !CarAvailabilityColumns.Contains(dataReader.GetName(i)))
                {
                    AttributeMap.Add(dataReader.GetName(i), i);
                    tmpData[i] = new List<float>();
                }
            }

            while (dataReader != null && dataReader.Read())
            {
                for (int i = 0; i < dataReader.FieldCount; i++)
                {
                    if (!IgnoreColumns.Contains(dataReader.GetName(i)) && !CarAvailabilityColumns.Contains(dataReader.GetName(i)))
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

        await using (var cmd = dataSource.CreateCommand($"SELECT * FROM scenario{scenario.CarAvailability}.total_all"))
        await using (var dataReader = cmd.ExecuteReader())
        {
            var AttributeMap = new Dictionary<string, int>(dataReader.FieldCount);
            var tmpData = new List<float>[dataReader.FieldCount];
            var piposId = new List<int>();

            for (int i = 0; i < dataReader.FieldCount; i++)
            {
                if (CarAvailabilityColumns.Contains(dataReader.GetName(i)))
                {
                    AttributeMap.Add(dataReader.GetName(i), i);
                    tmpData[i] = new List<float>();
                }
            }

            while (dataReader != null && dataReader.Read())
            {
                for (int i = 0; i < dataReader.FieldCount; i++)
                {
                    if (CarAvailabilityColumns.Contains(dataReader.GetName(i)))
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
                Dictionary<string, float>? row;

                if(!data.TryGetValue(piposId[i], out row))
                {
                    row = new Dictionary<string, float>();
                    data.Add(piposId[i], row);
                }
                foreach (var (name, index) in AttributeMap)
                {
                    row.Add(name, tmpData[index][i]);
                }                
            }
        }

        string json_out = JsonSerializer.Serialize(data);
        Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
        File.WriteAllText(filename, json_out);

        return new DataMatrix(data);
    }
}
