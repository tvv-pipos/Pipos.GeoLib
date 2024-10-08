
namespace Pipos.GeoLib.Core.Model;

public class YearSet
{
    public Int64 Set { get; private set; }

    public YearSet()
    {
        Set = 0;
    }
    public YearSet(YearSet yearSet)
    {
        Set = yearSet.Set;
    }
    public YearSet(Year year)
    {
        Set = year.YearBit;
    }
    public bool IsSingleYear()
    {
        return  Set == (Set & -Set);
    }
    public bool HasYear(Year year)
    {
        return (year.YearBit & Set) != 0;
    }
    public YearSet Add(YearSet sid)
    {
        Set |= sid.Set;
        return this;
    }
    public YearSet Add(Year year)
    {
        Set |= year.YearBit;
        return this;
    }
}