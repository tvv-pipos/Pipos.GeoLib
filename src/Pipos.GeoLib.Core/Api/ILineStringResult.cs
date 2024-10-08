using NetTopologySuite.Geometries;

namespace Pipos.GeoLib.Core.Api;

public interface ILineStringResult
{
    bool HasResult { get; }
    LineString LineString { get; }
}