using NetTopologySuite.Geometries;

namespace Pipos.GeoLib.Road;

public struct Attribute
{
    public UInt32 Value;
    public int Class 
    { 
        get
        {
            return (int)(Value & 0xFU);
        } 
        set
        {
            Value = (Value & ~0xFU) | ((uint)value & 0xFU);
        }
    }
    public bool Ferry
    {
        get
        {
            return (Value & 0x10U) != 0;
        }
        set
        {
            if(value)
                Value |= 0x10U;
            else
                Value &= ~0x10U;
        }
    }
    public bool ForwardProhibited
    {
        get
        {
            return (Value & 0x20U) != 0;
        }
        set
        {
            if(value)
                Value |= 0x20U;
            else
                Value &= ~0x20U;
        }
    }
    public bool BackwardProhibited
    {
        get
        {
            return (Value & 0x40U) != 0;
        }
        set
        {
            if(value)
                Value |= 0x40U;
            else
                Value &= ~0x40U;
        }
    }
    public bool Motorway
    {
        get
        {
            return (Value & 0x80U) != 0;
        }
        set
        {
            if(value)
                Value |= 0x80U;
            else
                Value &= ~0x80U;
        }
    }

    //public int ADT => (int)(Value >> 8);

    public Attribute()
    {
        Value = 0;
    }  
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
