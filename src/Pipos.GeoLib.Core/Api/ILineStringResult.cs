using NetTopologySuite.Geometries;

namespace Pipos.GeoLib.Core.Api;

public interface ILineStringResult
{
    bool HasResult { get; }
    public float Time { get; }
    public float Distance { get; }
    LineString LineString { get; }
}