namespace Pipos.GeoLib.Core.Api;

public interface ITimeResult
{
    bool HasResult { get; }
    float Time { get; }
}