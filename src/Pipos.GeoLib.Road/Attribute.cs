using NetTopologySuite.Geometries;

namespace Pipos.GeoLib.Road;

public struct Attribute
{
    public UInt32 Value;
    public int Class => (int)(Value & 0xF);
    public bool Ferry => (Value & 0x10) == 0x10;
    public bool ForwardProhibited => (Value & 0x20) == 0x20;
    public bool BackwardProhibited => (Value & 0x40) == 0x40;
    public bool Motorway => (Value & 0x80) == 0x80;
    public int ADT => (int)(Value >> 8);
    
    public Attribute(UInt32 value)
    {
        Value = value;
    }
    public Attribute Reverse()
    {
        UInt32 tmp = Value;
        if((tmp & 0x20) != ((tmp & 0x40) >> 1))
            tmp ^= (3 << 5);
        return new Attribute(tmp);
    }
}
