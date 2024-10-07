using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using Npgsql;
using Pipos.Common.NetworkUtilities.Model;
namespace Pipos.Common.NetworkUtilities.IO;

public class Sampers
{
    /*
        SELECT 
        geom, tile_250, pipos_id, 
        
        id, fid, area, id1, id_zon, area1, id2, vattenarea, 
        fhusyta, density, lan_c, kom_c, samp_reg, sthlm_ic, 
        ikea, uni_ort, l_nr, turistomr_, turistpunk, turist_omr, turist_pun, turist_om1
        
        FROM common.tile250innercoversweden;
    */
    public static string Id = "id";
    public static string Fid = "fid";
    public static string Area = "area";
    public static string Id1 = "id1";
    public static string IdZon = "id_zon";
    public static string Area1 = "area1";
    public static string Id2 = "id2";
    public static string Vattenarea = "vattenarea";
    public static string Fhusyta = "fhusyta";
    public static string Density = "density";
    public static string LanC = "lan_c";
    public static string KomC = "kom_c";
    public static string SampReg = "samp_reg";
    public static string SthlmIc = "sthlm_ic";
    public static string Ikea = "ikea";
    public static string UniOrt = "uni_ort";
    public static string LNr = "l_nr";
    public static string Turistomr = "turistomr_";
    public static string Turistpunk = "turistpunk";
    public static string TuristOmr = "turist_omr";
    public static string TuristPun = "turist_pun";
    public static string TuristOm1 = "turist_om1";

    private static readonly string[] IgnoreColumns = { "tile_250", "pipos_id", "geom", "id1", "id", "fid", "id1", "area1", "id2", "vattenarea", "fhusyta", "density", "lan_c", "ikea", "uni_ort", "l_nr", "turistomr_", "turistpunk", "turist_omr", "turist_pun", "turist_om1" };

    public async static Task<DataMatrix> Read(string connectionString, Scenario scenario)
    {
        string filename = $"{Settings.PiposDataSharePath}/0/Sampers.json";
        if(File.Exists(filename))
        {
            string json = File.ReadAllText(filename);
            return new DataMatrix(JsonSerializer.Deserialize<Dictionary<int, Dictionary<string, float>>>(json)!);
        }

        Dictionary<int, Dictionary<string, float>> data = new Dictionary<int, Dictionary<string, float>>();

        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        await using (var cmd = dataSource.CreateCommand($"SELECT * FROM common.tile250innercoversweden"))
        await using (var dataReader = cmd.ExecuteReader())
        {
            var AttributeMap = new Dictionary<string, int>(dataReader.FieldCount);
            var tmpData = new List<float>[dataReader.FieldCount];
            var piposIdSet = new List<int>();

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
                        tmpData[i].Add((float)dataReader.GetDouble(i));
                    }
                    if (dataReader.GetName(i).Equals("pipos_id"))
                    {
                        piposIdSet.Add(dataReader.GetInt32(i));
                    }
                }
            }

            var piposId = piposIdSet.ToArray();

            for (int i = 0; i < piposId.Length; i++)
            {
                Dictionary<string, float> row = new Dictionary<string, float>();
                foreach (var (name, index) in AttributeMap)
                {
                    row.Add(name, tmpData[index][i]);
                }
                data.Add(piposId[i], row);
            }
        }

        string json_out = JsonSerializer.Serialize(data);
        Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
        File.WriteAllText(filename, json_out);

        return new DataMatrix(data);
    }
}