namespace Pipos.GeoLib.Core.Api;

public interface IWithinDistanceResult
{
    bool HasResult { get; }
    IDistanceResult FindDistance(IConnection end, IQueryOptions options);
}