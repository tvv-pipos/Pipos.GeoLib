using Pipos.Common.NetworkUtilities.Model;

namespace Pipos.Common.NetworkUtilities.IO;

public static class PiposPath
{
    public enum Storage
    {
        File, 
        Database
    }

    public static (string, string) GetIndex(Scenario sceanrio, string transportmodel, Storage storage)
    {
        if (storage == Storage.File)
        {
            return ($"{Settings.PiposDataSharePath}/{sceanrio}/index", $"{transportmodel}");
        } 
        else// if(storage == Storage.Database)
        {
            return ($"scenario{sceanrio.ActivityTile}_index", $"{transportmodel}");
        }
    }
    public static (string, string) GetLogsum(Scenario sceanrio, string transportmodel, Storage storage)
    {
        if (storage == Storage.File)
        {
            return ($"{Settings.PiposDataSharePath}/{sceanrio}/logsum", $"{transportmodel}");
        }
        else// if (storage == Storage.Database)
        {
            return ($"scenario{sceanrio.ActivityTile}_logsum", $"{transportmodel}"); ;
        }
    }

    public static (string, string) GetNVDB(Scenario sceanrio, string transportmodel, Storage storage)
    {
        if (storage == Storage.File)
        {
            return ($"{Settings.PiposDataSharePath}/{sceanrio.NVDB}/nvdb", $"{transportmodel}");
        }
        else// if (storage == Storage.Database)
        {
            return ("", "");
        }
    }

    public static (string, string) GetGTFS(Scenario sceanrio, string transportmodel, Storage storage)
    {
        if (storage == Storage.File)
        {
            return ($"{Settings.PiposDataSharePath}/{sceanrio.GTFS}/gtfs", $"{transportmodel}");
        }
        else// if (storage == Storage.Database)
        {
            return ("", "");
        }
    }
}
