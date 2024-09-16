using Npgsql;
namespace Pipos.Common.NetworkUtilities.IO;
public static class TravelReason
{
    public static readonly string[] IgnoreColumns = { "tile_250", "pipos_id", "geom" };

    public struct CommonVariables
    {
        //string DatabaseName;
        public string Category;
        public string MenuName;
        public string IndexModell;
        public float MaxSearchDistance;
        public float TimeParameter;
    }

    /*public class TravelReasons
    {
       public int[] PiposId;
       public Dictionary<string, float[]> Data = new Dictionary<string, float[]>();
    }*/

    public async static Task<Dictionary<string, CommonVariables>> ReadCommonVariables(string connectionString)
    {
        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        Dictionary<string, CommonVariables> cv = new Dictionary<string, CommonVariables>();

        await using (var cmd = dataSource.CreateCommand($@"SELECT dataname, category, travel_reason, number_of_significance, max_traveldistance_car, max_traveltime_car FROM common.tr_common_variables WHERE status = 'Aktiv' ORDER BY dataname"))
        
        await using (var dataReader = cmd.ExecuteReader())
        {
            while (dataReader != null && dataReader.Read())
            {
                string name = dataReader.GetString(0);
                CommonVariables commonVariables = new CommonVariables();
                commonVariables.Category = dataReader.GetString(1);
                commonVariables.MenuName =  dataReader.GetString(2);
                commonVariables.IndexModell = dataReader.GetString(3);
                commonVariables.MaxSearchDistance = dataReader.GetFloat(4);
                commonVariables.TimeParameter = dataReader.GetFloat(5);
                cv.Add(name, commonVariables);
            }
        }
        return cv;
    }

    /// <summary>
    /// Read the travel reasons from the database
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="scenarioId"></param>
    /// <returns></returns>
    public static async Task<(int[], Dictionary<string, float[]>)> ReadTravelReasons(string connectionString, int scenarioId)
    {
        int[] piposId = null!;
        var trData = new Dictionary<string, float[]>();

        const string conditions = @"ST_Within(geom, ST_SetSRID(ST_GeomFromGeoJSON('{""type"":""Polygon"",""coordinates"":
            [[[489109.27917894686,6995947.364573294],[490593.49354063766,6993411.554385832],[492989.5346626497,6993478.111083665],
            [490560.2151917208,6996779.323296214],[489109.27917894686,6995947.364573294]]]}'), 3006))";
        var query = $"SELECT * FROM tr_scenario{scenarioId}.tr_total_all WHERE {conditions} ORDER BY pipos_id";
        
        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        await using (var cmd = dataSource.CreateCommand(query))
        await using (var dataReader = cmd.ExecuteReader())
        {
            var attributeMap = new Dictionary<string, int>(dataReader.FieldCount);
            var tmpData = new List<float>[dataReader.FieldCount];
            var tmpId = new List<int>();

            for (var i = 0; i < dataReader.FieldCount; i++)
            {
                if (!IgnoreColumns.Contains(dataReader.GetName(i)))
                {
                    attributeMap.Add(dataReader.GetName(i), i);
                    tmpData[i] = new List<float>();
                }
            }

            while (dataReader.Read())
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

            piposId = tmpId.ToArray();
            foreach (var (name, index) in attributeMap)
            {
                trData.Add(name, tmpData[index].ToArray());
            }
        }
        return (piposId, trData);
    }

    public async static Task<List<int>> ReadTravelReasonTiles(string connectionString, int scenario_id)
    {
        List<int> pipos_id = new List<int>();

        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        await using (var cmd = dataSource.CreateCommand($"SELECT * FROM tr_scenario{scenario_id}.tr_total_all ORDER BY pipos_id"))
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
    }
}
