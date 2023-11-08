namespace Pipos.Common.NetworkUtilities.Model;
public static class PiposID
{
    public static Int32 IdFromRutID(Int64 RutID)
    {
        Int32 oldx = (Int32)(RutID / 10000000);
        Int32 oldy = (Int32)(RutID - (oldx * 10000000));
        Int32 x = (oldx / 250);
        Int32 y = (oldy / 250);
        return ((0xFFFF & y) << 16) | (0xFFFF & x);
    }
    public static Int32 XFromId(Int32 id)
    {
        return (0xFFFF & id) * 250;
    }
    public static Int32 YFromId(Int32 id)
    {
        return ((id >> 16)) * 250;
    }
    public static Int32 IdFromXY(Int32 x, Int32 y)
    {
        return ((0xFFFF & (y / 250)) << 16) | (0xFFFF & (x / 250));
    }
    public static int XFromRutID(long RutID)
    {
        return (int)(RutID / 10000000);
    }
    public static int YFromRutID(long RutID)
    {
        return (int)(RutID - ((RutID / 10000000) * 10000000));
    }
}
