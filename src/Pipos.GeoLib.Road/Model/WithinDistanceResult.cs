using Pipos.GeoLib.Core.Api;
using Pipos.GeoLib.Core.Model;
using Pipos.GeoLib.Road.Model;
using static Pipos.GeoLib.Road.Dijkstra.NetworkUtils;

namespace Pipos.GeoLib.Road.Model;

public class WithinDistanceResult : IWithinDistanceResult
{
    public Dictionary<uint, float>? Weights { get; set; }
    public bool HasResult { get; set; }
    public IDistanceResult FindDistance(IConnection end, IQueryOptions options)
    {
        if(options is not QueryOptions)
            return DistanceResult.NoResult;

        return EndWeightsDistance((Connection)end, Weights!, (QueryOptions)options);
    }
    public static WithinDistanceResult NoResult = new WithinDistanceResult{HasResult = false};
}