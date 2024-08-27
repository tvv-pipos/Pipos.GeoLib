using Pipos.Common.NetworkUtilities.IO;

namespace Pipos.Napier.IO;

public interface IDistanceModel
{
    /// <summary>
    /// Saves the result from calculation
    /// </summary>
    /// <param name="scenarioId"></param>
    /// <param name="transportModel"></param>
    /// <param name="storage"></param>
    /// <param name="startId"></param>
    /// <param name="result"></param>
    Task SaveResultAsync(int scenarioId, string transportModel, PiposPath.Storage storage, int[] startId, Dictionary<string, float[]> result);
}