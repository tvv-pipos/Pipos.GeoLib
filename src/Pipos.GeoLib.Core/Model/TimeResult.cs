using Pipos.GeoLib.Core.Api;
namespace Pipos.GeoLib.Core.Model;

public class TimeResult : ITimeResult
{
    public float Time { get; set; }
    public bool HasResult { get; set; }

    public TimeResult() {}

    public static TimeResult NoResult = new TimeResult{HasResult = false};
}