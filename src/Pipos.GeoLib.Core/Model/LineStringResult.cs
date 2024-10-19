using NetTopologySuite.Geometries;
using Pipos.GeoLib.Core.Api;

namespace Pipos.GeoLib.Core.Model;

public class LineStringResult : ILineStringResult
{
    public bool HasResult { get; set; }
    public float Time { get; set; }
    public float Distance { get; set; }
    public LineString LineString { get; set; } = LineString.Empty;
    public LineStringResult() {}

    public static LineStringResult NoResult =  new LineStringResult{ HasResult = false };
}