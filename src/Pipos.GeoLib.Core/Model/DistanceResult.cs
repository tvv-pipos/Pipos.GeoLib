using Pipos.GeoLib.Core.Api;
namespace Pipos.GeoLib.Core.Model;

public class DistanceResult : IDistanceResult
{
    public float Distance { get; set; }
    public bool HasResult { get; set; }
    public DistanceResult() {}
    public static DistanceResult NoResult = new DistanceResult{HasResult = false};
}