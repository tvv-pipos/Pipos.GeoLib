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

        await using (var cmd = dataSource.CreateCommand($@"SELECT datanamn, kategori, reseanledning_ny, ""antal av betydelse"", ""max resavstånd bil"", ""max resavstånd tid bil"" FROM common.tr_common_variables"))
        
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

    public async static Task<(int[], Dictionary<string, float[]>)> ReadTravelReasons(string connectionString, int scenario_id)
    {
        int[] pipos_id = null!;
        Dictionary<string, float[]> tr_data = new Dictionary<string, float[]>();

        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        await using (var cmd = dataSource.CreateCommand($"SELECT * FROM tr_scenario{scenario_id}.tr_total_all"))
        await using (var dataReader = cmd.ExecuteReader())
        {
            var AttributeMap = new Dictionary<string, int>(dataReader.FieldCount);
            var tmpData = new List<float>[dataReader.FieldCount];
            var tmpId = new List<int>();

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
                        tmpId.Add(dataReader.GetInt32(i));
                    }
                }
            }

            pipos_id = tmpId.ToArray();
            foreach (var (name, index) in AttributeMap)
            {
                tr_data.Add(name, tmpData[index].ToArray());
            }
        }
        return (pipos_id, tr_data);
    }

    public async static Task<List<int>> ReadTravelReasonTiles(string connectionString, int scenario_id)
    {
        List<int> pipos_id = new List<int>();

        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        await using (var cmd = dataSource.CreateCommand($"SELECT * FROM tr_scenario{scenario_id}.tr_total_all"))
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
