using Pipos.GeoLib.Core.Model;

namespace Pipos.GeoLib.Core.Api;
public interface INetwork
{
    IConnectionIndex Connect { get; }

    IDistanceResult FindShortestDistance(IConnection start, IConnection end, Year year, IQueryOptions options);
    ITimeResult FindShortestTime(IConnection start, IConnection end, Year year, IQueryOptions options);
    ITimeDistanceResult FindShortestDistanceWithTime(IConnection start, IConnection end, Year year, IQueryOptions options);
    ITimeDistanceResult FindShortestTimeWithDistance(IConnection start, IConnection end, Year year, IQueryOptions options);
    ILineStringResult FindShortestDistanceLineString(IConnection start, IConnection end, Year year, IQueryOptions options);
    ILineStringResult FindShortestTimeLineString(IConnection start, IConnection end, Year year, IQueryOptions options);
    IWithinDistanceResult FindWithinDistance(IConnection start, float maxDistance, Year year, IQueryOptions options);
    IWithinTimeResult FindWithinTime(IConnection start, float maxTime, Year year, IQueryOptions options);
    IWithinTimeDistanceResult FindWithinDistanceWithTime(IConnection start, float maxDistance, Year year, IQueryOptions options);
    IWithinTimeDistanceResult FindWithinTimeWithDistance(IConnection start, float maxTime, Year year, IQueryOptions options);
    IWithinLineStringResult FindWithinLineString(IConnection start, float maxTime, Year year, IQueryOptions options);
}
