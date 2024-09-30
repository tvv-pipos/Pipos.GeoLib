using System.Text;
using System.Text.Json;
using Npgsql;
using Pipos.Common.NetworkUtilities.Model;
namespace Pipos.Common.NetworkUtilities.IO;
public static class TravelReason
{
    private static readonly string[] IgnoreColumns = { "tile_250", "pipos_id", "geom" };

    public struct CommonVariables
    {
        //string DatabaseName;
        public string Category { get; set; }
        public string MenuName { get; set; }
        public string IndexModell  { get; set; }
        public float MaxSearchDistance  { get; set; }
        public float TimeParameter  { get; set; }
    }

    public class TravelReasons
    {
       public int[] PiposId { get; set; }
       public Dictionary<string, float[]> Data { get; set; } = new Dictionary<string, float[]>();
    }

    /// <summary>
    /// Reads the common variables from DB
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="scenario"></param>
    /// <returns></returns>
    public static async Task<Dictionary<string, CommonVariables>> ReadCommonVariables(string connectionString, Scenario scenario)
    {
        var filename = $"{Settings.PiposDataSharePath}/0/ReadCommonVariables.json";
        if (File.Exists(filename))
        {
            var json = await File.ReadAllTextAsync(filename);
            return JsonSerializer.Deserialize<Dictionary<string, CommonVariables>>(json)!;
        }

        var cv = new Dictionary<string, CommonVariables>();
        const string query = $"""
                              SELECT dataname, category, travel_reason, number_of_significance, max_traveldistance_car, max_traveltime_car
                              FROM common.tr_common_variables WHERE status = 'Aktiv' ORDER BY dataname
                              """;
        
        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        await using (var cmd = dataSource.CreateCommand(query))
        await using (var dataReader = cmd.ExecuteReader())
        {
            while (await dataReader.ReadAsync())
            {
                var name = dataReader.GetString(0);
                var commonVariables = new CommonVariables
                                      {
                                          Category = dataReader.GetString(1),
                                          MenuName = dataReader.GetString(2),
                                          IndexModell = dataReader.GetString(3),
                                          MaxSearchDistance = dataReader.GetFloat(4),
                                          TimeParameter = dataReader.GetFloat(5)
                                      };
                cv.Add(name, commonVariables);
            }
        }

        var jsonOut = JsonSerializer.Serialize(cv);
        Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
        await File.WriteAllTextAsync(filename, jsonOut);
        return cv;
    }

    /// <summary>
    /// Read the travel reasons from the database
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="scenario"></param>
    /// <returns></returns>
    public static async Task<TravelReasons> ReadTravelReasons(string connectionString, Scenario scenario)
    {
        var filename = $"{Settings.PiposDataSharePath}/{scenario.ActivityTile}/ReadTravelReasons.json";
        if (File.Exists(filename))
        {
            var json = await File.ReadAllTextAsync(filename);
            var travelReasons = JsonSerializer.Deserialize<TravelReasons>(json)!;
            return travelReasons;
        }

        var tr = new TravelReasons();
        const string conditions = @"ST_Within(geom, ST_SetSRID(ST_GeomFromGeoJSON('{""type"":""Polygon"",""coordinates"":
            [[[489109.27917894686,6995947.364573294],[490593.49354063766,6993411.554385832],[492989.5346626497,6993478.111083665],
            [490560.2151917208,6996779.323296214],[489109.27917894686,6995947.364573294]]]}'), 3006))";
        var query = $"SELECT * FROM scenario{scenario.ActivityTile}_tr.tr_total_all WHERE {conditions} ORDER BY pipos_id";

        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        await using (var cmd = dataSource.CreateCommand(query))
        await using (var dataReader = cmd.ExecuteReader())
        {
            var attributeMap = new Dictionary<string, int>(dataReader.FieldCount);
            var tmpData = new List<float>[dataReader.FieldCount];
            var tmpId = new List<int>();

            for (var i = 0; i < dataReader.FieldCount; i++)
            {
                if (IgnoreColumns.Contains(dataReader.GetName(i)))
                    continue;
                
                attributeMap.Add(dataReader.GetName(i), i);
                tmpData[i] = [];
            }

            while (await dataReader.ReadAsync())
            {
                for (var i = 0; i < dataReader.FieldCount; i++)
                {
                    if (!IgnoreColumns.Contains(dataReader.GetName(i)))
                    {
                        tmpData[i].Add(dataReader.GetFloat(i));
                    }
                    if (dataReader.GetName(i).Equals("pipos_id"))
                    {
                        tmpId.Add(dataReader.GetInt32(i));
                    }
                }
            }

            tr.PiposId = tmpId.ToArray();
            foreach (var (name, index) in attributeMap)
            {
                tr.Data.Add(name, tmpData[index].ToArray());
            }
        }

        var jsonOut = JsonSerializer.Serialize(tr);
        Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
        await File.WriteAllTextAsync(filename, jsonOut);

        return tr;
    }

    /// <summary>
    /// Reads the travel reasons tiles
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="scenario"></param>
    /// <returns></returns>
    public static async Task<List<int>> ReadTravelReasonTiles(string connectionString, Scenario scenario)
    {
        var filename = $"{Settings.PiposDataSharePath}/{scenario.ActivityTile}/ReadTravelReasonTiles.json";
        if (File.Exists(filename))
        {
            var json = await File.ReadAllTextAsync(filename);
            return JsonSerializer.Deserialize<List<int>>(json)!;
        }

        var piposIds = new List<int>();
        var query = $"SELECT * FROM scenario{scenario.ActivityTile}_tr.tr_total_all ORDER BY pipos_id";

        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        await using (var cmd = dataSource.CreateCommand(query))
        await using (var dataReader = cmd.ExecuteReader())
        {
            while (await dataReader.ReadAsync())
            {
                for (var i = 0; i < dataReader.FieldCount; i++)
                {
                    if (dataReader.GetName(i).Equals("pipos_id"))
                    {
                        piposIds.Add(dataReader.GetInt32(i));
                    }
                }
            }
        }

        var jsonOut = JsonSerializer.Serialize(piposIds);
        Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
        await File.WriteAllTextAsync(filename, jsonOut);

        return piposIds;
    }
}
