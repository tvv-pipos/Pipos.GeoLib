using RoutingKit;
using System.Linq;
using Pipos.Common.NetworkUtilities.Model;
using Pipos.Common.NetworkUtilities.Processing;
using System.Diagnostics;

namespace Pipos.Common.NetworkUtilities.IO;

public static class Index
{
    public static void SaveResultToFile(string basepath, int scenario_id, string transportmodel)
    {
        var tablename = $"{basepath}/{PiposPath.GetIndex(_ScenarioId, _TransportModel.ToString(), PiposPath.Storage.File)}";
        WriteCSV(filename, result);
    }

    private static void WriteCSV(string filename, Dictionary<string, float[]> result)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filename)!);
        var csv = new StringBuilder();
        csv.Append("id, x, y");
        foreach (var (name, res) in result)
        {
            csv.Append($", {name}");
        }
        csv.AppendLine();

        for (int i = 0; i < _StartId.Length; i++)
        {
            csv.Append($"{_StartId[i]}, {XFromId(_StartId[i]) + 125}, {YFromId(_StartId[i]) + 125}");
            foreach (var (name, res) in result)
            {
                csv.Append($", {res[i].ToString(CultureInfo.InvariantCulture)}");
            }
            csv.AppendLine();
        }

        File.WriteAllText(filename, csv.ToString());
    }
}