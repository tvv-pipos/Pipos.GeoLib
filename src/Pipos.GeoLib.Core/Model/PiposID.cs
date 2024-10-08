using NetTopologySuite.Geometries;

namespace Pipos.GeoLib.Core.Model;

public static class PiposID
{
    public static UInt32 Id(UInt64 RutID)
    {
        UInt32 oldx = (UInt32)(RutID / 10000000);
        UInt32 oldy = (UInt32)(RutID - (oldx * 10000000));
        UInt32 x = (oldx / 250);
        UInt32 y = (oldy / 250);
        return ((0xFFFF & y) << 16) | (0xFFFF & x);
    }
    public static UInt32 Id(UInt32 x, UInt32 y)
    {
        return ((0xFFFF & (y / 250)) << 16) | (0xFFFF & (x / 250));
    }
    public static UInt32 X(UInt32 id)
    {
        return (0xFFFF & id) * 250;
    }
    public static UInt32 Y(UInt32 id)
    {
        return ((id >> 16)) * 250;
    }
    public static NetTopologySuite.Geometries.Polygon PolygonFromId(int id)
    {
        int x = (0xFFFF & id) * 250;
        int y = ((id >> 16)) * 250;
        return new NetTopologySuite.Geometries.Polygon(
        new LinearRing(new Coordinate[]
        {
            new Coordinate(x,       y      ),
            new Coordinate(x + 250, y      ),
            new Coordinate(x + 250, y + 250),
            new Coordinate(x,       y + 250),
            new Coordinate(x,       y      )
        }));
    }
}
