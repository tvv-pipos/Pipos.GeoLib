namespace Pipos.GeoLib.Core.Api;

public interface IWithinLineStringResult
{
    bool HasResult { get; }
    ILineStringResult FindLineString(IConnection end, IQueryOptions options);
}