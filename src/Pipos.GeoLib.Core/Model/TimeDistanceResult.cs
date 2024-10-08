using Pipos.GeoLib.Core.Api;
namespace Pipos.GeoLib.Core.Model;

public class TimeDistanceResult : ITimeDistanceResult
{
    public float Time { get; set; }
    public float Distance { get; set; }
    public bool HasResult { get; set; }

    public TimeDistanceResult() {}
    public static TimeDistanceResult NoResult = new TimeDistanceResult{HasResult = false};
}