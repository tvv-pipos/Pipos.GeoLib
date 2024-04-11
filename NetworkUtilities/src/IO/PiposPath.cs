using Pipos.Common.NetworkUtilities.Model;

namespace Pipos.Common.NetworkUtilities.IO;

public static class PiposPath
{
    public enum Storage
    {
        File, 
        Database
    }

    public static (string, string) GetIndex(int sceanrio_id, string transportmodel, Storage storage)
    {
        if (storage == Storage.File)
        {
            return ($"{Settings.PiposDataSharePath}/{sceanrio_id}/index", $"{transportmodel}");
        } 
        else// if(storage == Storage.Database)
        {
            return ($"scenario{sceanrio_id}_index", $"{transportmodel}");
        }
    }
    public static (string, string) GetLogsum(int sceanrio_id, string transportmodel, Storage storage)
    {
        if (storage == Storage.File)
        {
            return ($"{Settings.PiposDataSharePath}/{sceanrio_id}/logsum", $"{transportmodel}");
        }
        else// if (storage == Storage.Database)
        {
            return ($"scenario{sceanrio_id}_logsum", $"{transportmodel}"); ;
        }
    }

    public static (string, string) GetNetwork(int sceanrio_id, string transportmodel, Storage storage)
    {
        if (storage == Storage.File)
        {
            return ($"{Settings.PiposDataSharePath}/{sceanrio_id}/network", $"{transportmodel}");
        }
        else// if (storage == Storage.Database)
        {
            return ("", "");
        }
    }
}
