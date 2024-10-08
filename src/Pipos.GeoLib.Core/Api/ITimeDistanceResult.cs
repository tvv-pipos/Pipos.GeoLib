namespace Pipos.GeoLib.Core.Api;

public interface ITimeDistanceResult
{
    bool HasResult { get; }
    float Distance { get; }
    float Time { get; }
}