using Pipos.Common.NetworkUtilities.Model;
using System.Globalization;
using System.Text;
using Npgsql;

using static Pipos.Common.NetworkUtilities.Model.PiposID;

namespace Pipos.Common.NetworkUtilities.IO;

public static class LogsumModel
{
    public static void SaveResult(int scenario_id, string transportmodel, PiposPath.Storage storage, int[] startId, Dictionary<string, float[]> result)
    {
        var (path, name) = PiposPath.GetLogsum(scenario_id, transportmodel, storage);
        if (storage == PiposPath.Storage.File)
            SaveResultToFile(path, name, startId, result);
        else
            SaveResultToDB(path, name, startId, result);
    }

    private static void SaveResultToFile(string path, string filename, int[] startId, Dictionary<string, float[]> result)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException($"{nameof(path)} cannot be null or empty");
        }
        if (string.IsNullOrEmpty(filename))
        {
            throw new ArgumentException($"{nameof(filename)} cannot be null or empty");
        }

        var directoryName = Path.GetDirectoryName($"{path}/{filename}");

        if (directoryName == null)
        {
            throw new ArgumentException($"The directory name {directoryName} does not exists");
        }

        Directory.CreateDirectory(directoryName);
        var csv = new StringBuilder();
        csv.Append("id, x, y");
        foreach (var (name, res) in result)
        {
            csv.Append($", {name}");
        }
        csv.AppendLine();

        for (int i = 0; i < startId.Length; i++)
        {
            csv.Append($"{startId[i]}, {PiposID.XFromId(startId[i]) + 125}, {PiposID.YFromId(startId[i]) + 125}");
            foreach (var (name, res) in result)
            {
                csv.Append($", {res[i].ToString(CultureInfo.InvariantCulture)}");
            }
            csv.AppendLine();
        }

        File.WriteAllText($"{path}/{filename}", csv.ToString());
    }

    private static void SaveResultToDB(string schema, string table, int[] startId, Dictionary<string, float[]> result)
    {
        StringBuilder create_table = new StringBuilder($"CREATE TABLE {schema}.{table} (", 2048);
        StringBuilder columns = new StringBuilder(1024);

        foreach (var (column, value) in result)
        {
            create_table.Append($"{column} Real, ");
            columns.Append($"{column},");
        }
        create_table.Append("geom GEOMETRY, ");
        columns.Append($"geom,");

        create_table.Append("pipos_id INTEGER PRIMARY KEY)");
        columns.Append($"pipos_id");

        string copy = $"COPY {schema}.{table} ({columns.ToString()}) from STDIN (FORMAT BINARY)";


        var dataSourceBuilder = new NpgsqlDataSourceBuilder(Settings.PiposRutDataConnectionString);
            dataSourceBuilder.UseNetTopologySuite();
        var datasource = dataSourceBuilder.Build();

        using (NpgsqlConnection connection = datasource.OpenConnection())
        {
            NpgsqlCommand cmd = new NpgsqlCommand($"CREATE SCHEMA IF NOT EXISTS {schema}", connection);
            cmd.ExecuteNonQuery();

            cmd = new NpgsqlCommand($"DROP TABLE IF EXISTS {schema}.{table}", connection);
            cmd.ExecuteNonQuery();

            cmd = new NpgsqlCommand(create_table.ToString(), connection);
            cmd.ExecuteNonQuery();

            using (var writer = connection.BeginBinaryImport(copy))
            {
                for (int i = 0; i < startId.Length; i++)
                {
                    writer.StartRow();
                    foreach (var (column, value) in result)
                    {
                        writer.Write(value[i]);
                    }
                    writer.Write(PolygonFromId(startId[i]), NpgsqlTypes.NpgsqlDbType.Geometry);
                    writer.Write(startId[i]);
                }
                writer.Complete();
            }
        }
    }




}