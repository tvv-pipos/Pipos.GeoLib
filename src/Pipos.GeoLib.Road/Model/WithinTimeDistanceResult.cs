using Pipos.GeoLib.Core.Api;
using Pipos.GeoLib.Core.Model;
using Pipos.GeoLib.Road.Model;
using static Pipos.GeoLib.Road.Dijkstra.NetworkUtils;

namespace Pipos.GeoLib.Road.Model;

public class WithinTimeDistanceResult : IWithinTimeDistanceResult
{
    public Dictionary<uint, TimeDistanceResult>? Weights { get; set; }
    public bool HasResult { get; set; }
    public ITimeDistanceResult FindTimeDistance(IConnection end, IQueryOptions options)
    {
        if(options is not QueryOptions)
            return TimeDistanceResult.NoResult;

        return EndWeightsDistanceWithTime((Connection)end, Weights!, (QueryOptions)options);
    }
    public static WithinTimeDistanceResult NoResult = new WithinTimeDistanceResult{HasResult = false};
}