namespace Pipos.GeoLib.Core.Api;

public interface IWithinTimeResult
{
    bool HasResult { get; }
    ITimeResult FindTime(IConnection end, IQueryOptions options);
}