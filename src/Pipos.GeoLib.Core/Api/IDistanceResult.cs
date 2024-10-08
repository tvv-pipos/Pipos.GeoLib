namespace Pipos.GeoLib.Core.Api;

public interface IDistanceResult
{
    bool HasResult { get; }
    float Distance { get; }
}