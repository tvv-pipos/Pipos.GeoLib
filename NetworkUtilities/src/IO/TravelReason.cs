using System.Text;
using System.Text.Json;
using Npgsql;
using Pipos.Common.NetworkUtilities.Model;
namespace Pipos.Common.NetworkUtilities.IO;
public static class TravelReason
{
    public static readonly string[] IgnoreColumns = { "tile_250", "pipos_id", "geom" };

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

    public async static Task<Dictionary<string, CommonVariables>> ReadCommonVariables(string connectionString, Scenario scenario)
    {
        string filename = $"{Settings.PiposDataSharePath}/0/ReadCommonVariables.json";
        if(File.Exists(filename))
        {
            string json = File.ReadAllText(filename);
            return JsonSerializer.Deserialize<Dictionary<string, CommonVariables>>(json)!;
        }

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

        string json_out = JsonSerializer.Serialize(cv);
        Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
        File.WriteAllText(filename, json_out);
        return cv;
    }

    public async static Task<TravelReasons> ReadTravelReasons(string connectionString, Scenario scenario)
    {
        string filename = $"{Settings.PiposDataSharePath}/{scenario.ActivityTile}/ReadTravelReasons.json";
        if(File.Exists(filename))
        {
            string json = File.ReadAllText(filename);
            var travel_resons = JsonSerializer.Deserialize<TravelReasons>(json)!;

            /*string csv_file = $"{Settings.PiposDataSharePath}/{scenario.ActivityTile}/TravelReason.csv";
            using (var stream = File.CreateText(csv_file))
            {
                string[] colums = {"_1sniemployee","_2sniemployee","_3sniemployee","_4sniemployee","_5sniemployee","_6sniemployee","_7sniemployee","_8sniemployee","_9sniemployee","_10sniemployee","_11sniemployee","_12sniemployee","_13sniemployee","_14sniemployee","_15sniemployee","_16sniemployee","_101preschool","_102lowelemetry","_103mediumelementry","_104highelementry","_105highschool","_106highscholl_prof","_107university","_108folkhogskola","_109municipal_adult_bas","_110municipal_adult_highschool","_111municipal_adult_sfi","_200groceryday","_201groceryweek","_202grocerymonth","_203pharmacy","_205fuel","_210kontantuttag","_211kontantinsattning","_212betalningsformedling","_215package_submission","_216package_delivery","_301scarbformedling","_302scfkassan","_303scmigraverket","_304scpensionmynd","_305scskatteverket","_306policepresence","_307police_notification","_308police_passport","_320firestation","_401tandvard","_402bvc","_403sjukgymnastik","_404barnmorska","_405specialistmodravard","_406akutverksamhet","_407akuttandvard","_408tandhygienist","_409akutvuxenpsykiatri","_410akutbarnpsykiatri","_411forlossning","_412pharmacy","_413pharmacy_and_agent","_414healthcenter","_501skyddomr","_502swimarea","_503museum","_504library","_505cinema"};
                stream.WriteLine("id,geom,tile_250,pipos_id,_1sniemployee,_2sniemployee,_3sniemployee,_4sniemployee,_5sniemployee,_6sniemployee,_7sniemployee,_8sniemployee,_9sniemployee,_10sniemployee,_11sniemployee,_12sniemployee,_13sniemployee,_14sniemployee,_15sniemployee,_16sniemployee,_101preschool,_102lowelemetry,_103mediumelementry,_104highelementry,_105highschool,_106highscholl_prof,_107university,_108folkhogskola,_109municipal_adult_bas,_110municipal_adult_highschool,_111municipal_adult_sfi,_200groceryday,_201groceryweek,_202grocerymonth,_203pharmacy,_205fuel,_210kontantuttag,_211kontantinsattning,_212betalningsformedling,_215package_submission,_216package_delivery,_301scarbformedling,_302scfkassan,_303scmigraverket,_304scpensionmynd,_305scskatteverket,_306policepresence,_307police_notification,_308police_passport,_320firestation,_401tandvard,_402bvc,_403sjukgymnastik,_404barnmorska,_405specialistmodravard,_406akutverksamhet,_407akuttandvard,_408tandhygienist,_409akutvuxenpsykiatri,_410akutbarnpsykiatri,_411forlossning,_412pharmacy,_413pharmacy_and_agent,_414healthcenter,_501skyddomr,_502swimarea,_503museum,_504library,_505cinema");
                for (int i = 0; i < travel_resons.PiposId.Length; i++)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(i);
                    sb.Append(",");

                    sb.Append(",");
                    sb.Append(",");

                    sb.Append(travel_resons.PiposId[i]);
                    sb.Append(",");

                    for(int j = 0; j < colums.Length - 1; j++)
                    {
                        sb.Append(travel_resons.Data[colums[j]][i]);
                        sb.Append(",");
                    }

                    sb.Append(travel_resons.Data[colums[colums.Length - 1]][i]);
                    stream.WriteLine(sb.ToString());
                }
            }*/

            return travel_resons;
        }

        TravelReasons tr = new TravelReasons();

        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        await using (var cmd = dataSource.CreateCommand($"SELECT * FROM tr_scenario{scenario.ActivityTile}.tr_total_all ORDER BY pipos_id"))
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

            tr.PiposId = tmpId.ToArray();
            foreach (var (name, index) in AttributeMap)
            {
                tr.Data.Add(name, tmpData[index].ToArray());
            }
        }

        string json_out = JsonSerializer.Serialize(tr);
        Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
        File.WriteAllText(filename, json_out);

        return tr;
    }

    public async static Task<List<int>> ReadTravelReasonTiles(string connectionString, Scenario scenario)
    {
        string filename = $"{Settings.PiposDataSharePath}/{scenario.ActivityTile}/ReadTravelReasonTiles.json";
        if(File.Exists(filename))
        {
            string json = File.ReadAllText(filename);
            return JsonSerializer.Deserialize<List<int>>(json)!;
        }

        List<int> pipos_id = new List<int>();

        await using var dataSource = NpgsqlDataSource.Create(connectionString);
        await using (var cmd = dataSource.CreateCommand($"SELECT * FROM tr_scenario{scenario.ActivityTile}.tr_total_all ORDER BY pipos_id"))
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

        string json_out = JsonSerializer.Serialize(pipos_id);
        Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
        File.WriteAllText(filename, json_out);

        return pipos_id;
    }
}
