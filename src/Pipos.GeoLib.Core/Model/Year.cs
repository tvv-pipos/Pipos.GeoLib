using System.Diagnostics;

namespace Pipos.GeoLib.Core.Model;

public class Year
{
    public Int64 YearBit { get; private set; }

    public Year(int year)
    {
        Debug.Assert(year >= 2000 && year <= 2062);
        YearBit = (0x01L << (year - 2000));
    }
}