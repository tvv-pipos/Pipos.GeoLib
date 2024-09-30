using Pipos.Common.NetworkUtilities.IO;
using Pipos.Common.NetworkUtilities.Model;

namespace Pipos.Napier.IO;

public interface IDistanceModel
{
    /// <summary>
    /// Saves the result from calculation
    /// </summary>
    /// <param name="scenario"></param>
    /// <param name="transportModel"></param>
    /// <param name="storage"></param>
    /// <param name="startId"></param>
    /// <param name="result"></param>
    Task SaveResultAsync(Scenario scenario, string transportModel, PiposPath.Storage storage, int[] startId, Dictionary<string, float[]> result);
}