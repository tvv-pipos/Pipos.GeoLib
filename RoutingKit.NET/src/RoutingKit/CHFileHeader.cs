using System.Runtime.InteropServices;

namespace RoutingKit;

public static class CHFileHeaderConstants
{
    public const long CHMagicNumber = 0x436f6e7448696572;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CHFileHeader
{
    public long MagicNumber;
    public int NodeCount;
    public int ForwardArcCount;
    public int BackwardArcCount;
}
