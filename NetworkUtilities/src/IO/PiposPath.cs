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
    /// <param name="scenarioId"></param>
    /// <param name="transportModel"></param>
    /// <param name="storage"></param>
    /// <returns></returns>
    public static (string, string) GetIndex(int scenarioId, string transportModel, Storage storage)
    {
        return storage == Storage.File
            ? ($"{Settings.PiposDataSharePath}/{scenarioId}/index", $"{transportModel}")
            : ($"scenario{scenarioId}_index", $"{transportModel}");
    }
    
    /// <summary>
    /// Get path or schema for logsum
    /// </summary>
    /// <param name="scenarioId"></param>
    /// <param name="transportModel"></param>
    /// <param name="storage"></param>
    /// <returns></returns>
    public static (string, string) GetLogsum(int scenarioId, string transportModel, Storage storage)
    {
        return storage == Storage.File
            ? ($"{Settings.PiposDataSharePath}/{scenarioId}/logsum", $"{transportModel}")
            : ($"scenario{scenarioId}_logsum", $"{transportModel}");
    }

    /// <summary>
    /// Get path or schema for closest
    /// </summary>
    /// <param name="scenarioId"></param>
    /// <param name="transportModel"></param>
    /// <param name="storage"></param>
    /// <returns></returns>
    public static (string, string) GetClosest(int scenarioId, string transportModel, Storage storage)
    {
        return storage == Storage.File
            ? ($"{Settings.PiposDataSharePath}/{scenarioId}/closest", $"{transportModel}")
            : ($"scenario{scenarioId}_closest", $"{transportModel}");
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
