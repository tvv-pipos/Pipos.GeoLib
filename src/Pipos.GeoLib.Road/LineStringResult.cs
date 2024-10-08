using NetTopologySuite.Geometries;
using Pipos.GeoLib.Core.Api;

namespace Pipos.GeoLib.Road;

public class LineStringResult : ILineStringResult
{
    public bool HasResult { get; set; }
    public LineString LineString { get; set; } = LineString.Empty;
    public LineStringResult() {}

    public static LineStringResult NoResult =  new LineStringResult{ HasResult = false };
}