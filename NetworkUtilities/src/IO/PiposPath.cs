using Npgsql;
namespace Pipos.Common.NetworkUtilities.IO;

public static class PiposPath
{
    public enum Storage
    {
        File, 
        Database
    }

    public static string GetIndex(int sceanrio_id, string transportmodel, Storage storage)
    {
        if (storage == Storage.File)
        {
            return $"{sceanrio_id}/index/{transportmodel}";
        } 
        else if(storage == Storage.Database)
        {
            return $"sceanrio{sceanrio_id}_index.{transportmodel}";
        }
        return "";
    }
    public static string GetLogsum(int sceanrio_id, string travelreason, string transportmodel, Storage storage)
    {
        if (storage == Storage.File)
        {
            return $"{sceanrio_id}/logsum/{travelreason}_{transportmodel}";
        }
        else if (storage == Storage.Database)
        {
            return $"sceanrio{sceanrio_id}_logsum.{transportmodel}"; ;
        }
        return "";
    }

    public static string GetNetwork(int sceanrio_id, string transportmodel, Storage storage)
    {
        if (storage == Storage.File)
        {
            return $"{sceanrio_id}/network/{transportmodel}";
        }
        else if (storage == Storage.Database)
        {
            return "";
        }
        return "";
    }
}
