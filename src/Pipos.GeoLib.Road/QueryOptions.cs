
using System.Diagnostics;

namespace Pipos.GeoLib.Road;

public class QueryOptions
{
    public bool IncludeConnectionDistance { get; set; }

    // Speed in km/h
    public float ConnectionSpeed { get; set; }
}