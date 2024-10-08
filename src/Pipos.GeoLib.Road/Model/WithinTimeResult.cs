using Pipos.GeoLib.Core.Api;
using Pipos.GeoLib.Core.Model;
using Pipos.GeoLib.Road.Model;
using static Pipos.GeoLib.Road.Dijkstra.NetworkUtils;

namespace Pipos.GeoLib.Road.Model;
public class WithinTimeResult : IWithinTimeResult
{
    public Dictionary<uint, float>? Weights { get; set; }
    public bool HasResult { get; set; }
    public ITimeResult FindTime(IConnection end, IQueryOptions options)
    {
        if(options is not QueryOptions)
            return TimeResult.NoResult;
            
        return EndWeightsTime((Connection)end, Weights!, (QueryOptions)options);
    }
    public static WithinTimeResult NoResult = new WithinTimeResult{HasResult = false};
}