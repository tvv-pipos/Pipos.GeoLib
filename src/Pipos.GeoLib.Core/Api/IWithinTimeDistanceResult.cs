namespace Pipos.GeoLib.Core.Api;

public interface IWithinTimeDistanceResult
{
    bool HasResult { get; }
    ITimeDistanceResult FindTimeDistance(IConnection end, IQueryOptions options);
}