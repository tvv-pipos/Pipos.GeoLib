using Pipos.GeoLib.Core.Api;
using Pipos.GeoLib.Core.Model;
using Pipos.GeoLib.Road.Model;
using static Pipos.GeoLib.Road.Dijkstra.NetworkUtils;

namespace Pipos.GeoLib.Road.Model;
public class WithinLineStringResult : IWithinLineStringResult
{
    //public Dictionary<uint, float>? Weights { get; set; }
    public bool HasResult { get; set; }
    public ILineStringResult FindLineString(IConnection end, IQueryOptions options)
    {
        //if(options is not QueryOptions)
            return LineStringResult.NoResult;
            
        //return EndWeightsTime((Connection)end, Weights!, (QueryOptions)options);
    }
    public static WithinLineStringResult NoResult = new WithinLineStringResult{HasResult = false};
}