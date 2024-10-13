using Pipos.GeoLib.Core.Api;

namespace Pipos.GeoLib.Road;

public class QueryOptions : IQueryOptions
{
    public bool IncludeConnectionDistance { get; set; }

    // Speed in km/h
    public float ConnectionSpeed { get; set; }
}