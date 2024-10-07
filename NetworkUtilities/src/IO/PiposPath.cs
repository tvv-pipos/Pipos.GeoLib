using Pipos.Common.NetworkUtilities.Model;

namespace Pipos.Common.NetworkUtilities.IO;

public static class PiposPath
{
    /// <summary>
    /// Storage type for the result
    /// </summary>
    public enum Storage
    {
        File, 
        Database
    }

    /// <summary>
    /// Get path or schema for index
    /// </summary>
    /// <param name="scenario"></param>
    /// <param name="transportModel"></param>
    /// <param name="storage"></param>
    /// <returns></returns>
    public static (string, string) GetIndex(Scenario scenario, string transportModel, Storage storage)
    {
        return storage == Storage.File
            ? ($"{Settings.PiposDataSharePath}/{scenario}/index", $"{transportModel}")
            : ($"scenario{scenario.ActivityTile}_index", $"{transportModel}");
    }

    /// <summary>
    /// Get path or schema for logsum
    /// </summary>
    /// <param name="scenario"></param>
    /// <param name="transportModel"></param>
    /// <param name="storage"></param>
    /// <returns></returns>
    public static (string, string) GetLogsum(Scenario scenario, string transportModel, Storage storage)
    {
        return storage == Storage.File
            ? ($"{Settings.PiposDataSharePath}/{scenario}/logsum", $"{transportModel}")
            : ($"scenario{scenario.ActivityTile}_logsum", $"{transportModel}");
    }

    /// <summary>
    /// Get path or schema for closest
    /// </summary>
    /// <param name="scenario"></param>
    /// <param name="transportModel"></param>
    /// <param name="storage"></param>
    /// <returns></returns>
    public static (string, string) GetClosest(Scenario scenario, string transportModel, Storage storage)
    {
        return storage == Storage.File
            ? ($"{Settings.PiposDataSharePath}/{scenario}/closest", $"{transportModel}")
            : ($"scenario{scenario.ActivityTile}_closest", $"{transportModel}");
    }

    /// <summary>
    /// Gets path or schema for NVDB
    /// </summary>
    /// <param name="sceanrio"></param>
    /// <param name="transportmodel"></param>
    /// <param name="storage"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Gets path or schema for GTFS
    /// </summary>
    /// <param name="sceanrio"></param>
    /// <param name="transportmodel"></param>
    /// <param name="storage"></param>
    /// <returns></returns>
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
