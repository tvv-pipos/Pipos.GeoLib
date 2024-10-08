using NetTopologySuite.Geometries;

namespace Pipos.GeoLib.Core.Api;

public interface IConnection
{
    bool IsConnected();
}