using System.Net;
using System.Text;
using NetTopologySuite.Geometries;
using Pipos.GeoLib.Core.Api;
using Pipos.GeoLib.Core.Model;
using Pipos.GeoLib.Road.Model;
using Pipos.GeoLib.Road.Dijkstra;

namespace Pipos.GeoLib.Road;

public class Network : INetwork
{
    public IConnectionIndex Connect { get; set; }

    public Network(ConnectionIndex index)
    {
        Connect = index;
    }
    public IDistanceResult FindShortestDistance(IConnection start, IConnection end, Year year, IQueryOptions options)
    {
        if(!start.IsConnected() || !end.IsConnected() || start is not Connection || end is not Connection || options is not QueryOptions)
            return DistanceResult.NoResult; 

        var startC = (Connection)start;
        var endC = (Connection)end;
   
        return ShortestDistance.Query(startC, endC, year, (QueryOptions)options);
    }
    public ITimeResult FindShortestTime(IConnection start, IConnection end, Year years, IQueryOptions options)
    {
        if(!start.IsConnected() || !end.IsConnected() || start is not Connection || end is not Connection || options is not QueryOptions)
            return TimeResult.NoResult;

        var startC = (Connection)start;
        var endC = (Connection)end;

        return ShortestTime.Query(startC, endC, years, (QueryOptions)options);
    }
    public ITimeDistanceResult FindShortestDistanceWithTime(IConnection start, IConnection end, Year years, IQueryOptions options)
    {
        if(!start.IsConnected() || !end.IsConnected() || start is not Connection || end is not Connection || options is not QueryOptions)
            return TimeDistanceResult.NoResult;

        var startC = (Connection)start;
        var endC = (Connection)end;

        return ShortestDistanceWithTime.Query(startC, endC, years, (QueryOptions)options);
    }
    public ITimeDistanceResult FindShortestTimeWithDistance(IConnection start, IConnection end, Year year, IQueryOptions options)
    {
        if(!start.IsConnected() || !end.IsConnected() || start is not Connection || end is not Connection || options is not QueryOptions)
            return TimeDistanceResult.NoResult;

        var startC = (Connection)start;
        var endC = (Connection)end;

        return ShortestTimeWithDistance.Query(startC, endC, year, (QueryOptions)options);
    }
    public IWithinDistanceResult FindWithinDistance(IConnection start, float maxDistance, Year year, IQueryOptions options)
    {
        if(!start.IsConnected() || start is not Connection || options is not QueryOptions)
            return WithinDistanceResult.NoResult;

        var startC = (Connection)start;

        return WithinDistance.Query(startC, maxDistance, year, (QueryOptions)options);
    }
    public IWithinTimeResult FindWithinTime(IConnection start, float maxTime, Year year, IQueryOptions options)
    {
        if(!start.IsConnected() || start is not Connection || options is not QueryOptions)
            return WithinTimeResult.NoResult;

        var startC = (Connection)start;
        return WithinTime.Query(startC, maxTime, year, (QueryOptions)options);
    }
    public ILineStringResult FindShortestDistanceLineString(IConnection start, IConnection end, Year year, IQueryOptions options)
    {
        if(!start.IsConnected() || !end.IsConnected() || start is not Connection || end is not Connection || options is not QueryOptions)
            return LineStringResult.NoResult;

        var startC = (Connection)start;
        var endC = (Connection)end;
        return ShortestDistanceLineString.Query(startC, endC, year, (QueryOptions)options);
    }
    public ILineStringResult FindShortestTimeLineString(IConnection start, IConnection end, Year year, IQueryOptions options)
    {
        if(!start.IsConnected() || !end.IsConnected() || start is not Connection || end is not Connection || options is not QueryOptions)
            return LineStringResult.NoResult;

        var startC = (Connection)start;
        var endC = (Connection)end;
        return ShortestTimeLineString.Query(startC, endC, year, (QueryOptions)options);
    }
    public IWithinTimeDistanceResult FindWithinDistanceWithTime(IConnection start, float maxDistance, Year year, IQueryOptions options)
    {
        if(!start.IsConnected() || start is not Connection || options is not QueryOptions)
            return WithinTimeDistanceResult.NoResult;

        var startC = (Connection)start;
        return WithinDistanceWithTime.Query(startC, maxDistance, year, (QueryOptions)options);
    }
    public IWithinTimeDistanceResult FindWithinTimeWithDistance(IConnection start, float maxTime, Year year, IQueryOptions options)
    {
        if(!start.IsConnected() || start is not Connection || options is not QueryOptions)
            return WithinTimeDistanceResult.NoResult;

        var startC = (Connection)start;
        return WithinTimeWithDistance.Query(startC, maxTime, year, (QueryOptions)options);
    }
    public IWithinLineStringResult FindWithinLineString(IConnection start, float maxTime, Year year, IQueryOptions options)
    {
        return WithinLineStringResult.NoResult;
    }

}
