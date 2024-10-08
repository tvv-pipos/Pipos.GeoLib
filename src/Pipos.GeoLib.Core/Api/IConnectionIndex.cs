using NetTopologySuite.Geometries;
using Pipos.GeoLib.Core.Model;

namespace Pipos.GeoLib.Core.Api;

public interface IConnectionIndex
{
    IConnection Point(float x, float y, float radius, Year year, IConnectionRule rule);
    IConnection Point(Point point, float radius, Year year, IConnectionRule rule);
    List<IConnection> Points(List<Point> points, float radius, Year year, IConnectionRule rule);
    IConnection PiposId(uint piposId, float radius, Year year, IConnectionRule rule);
    List<IConnection> PiposIds(List<uint> piposIds, float radius, Year year, IConnectionRule rule);
}