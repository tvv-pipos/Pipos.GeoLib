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
    /// Reads the travel reasons from DB
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="scenarioId"></param>
    /// <returns></returns>
    public static async Task<(int[], Dictionary<string, float[]>)> ReadTravelReasons(string connectionString, int scenarioId)
    {
        int[] piposId = null!;
        var trData = new Dictionary<string, float[]>();

        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        await using var cmd = dataSource.CreateCommand($"SELECT * FROM scenario{scenarioId}_tr.tr_total_all ORDER BY pipos_id");
        await using var dataReader = cmd.ExecuteReader();
        
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
            for (int i = 0; i < dataReader.FieldCount; i++)
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

        return (piposId, trData);
    }

    /// <summary>
    /// Reads the square tiles travel reasons
    /// </summary>
    /// <param name="connectionString"></param>
    /// <param name="scenarioId"></param>
    /// <returns></returns>
    public static async Task<List<int>> ReadTravelReasonTiles(string connectionString, int scenarioId)
    {
        var piposId = new List<int>();

        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        await using var cmd = dataSource.CreateCommand($"SELECT * FROM scenario{scenarioId}_tr.tr_total_all ORDER BY pipos_id");
        await using var dataReader = cmd.ExecuteReader();
        
        while (dataReader.Read())
        {
            for (var i = 0; i < dataReader.FieldCount; i++)
            {
                if (dataReader.GetName(i).Equals("pipos_id"))
                {
                    piposId.Add(dataReader.GetInt32(i));
                }
            }
        }

        return piposId;
    }
}
