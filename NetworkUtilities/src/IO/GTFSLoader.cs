using Pipos.Common.NetworkUtilities.Model;
using GTFS;

namespace Pipos.Common.NetworkUtilities.IO;

public class GTFSLoader
{   
    public GTFSData ReadData(string gtfsFeedFilename)
    {
        var reader = new GTFSReader<GTFSFeed>();
        var feed = reader.Read(gtfsFeedFilename);

        var stopTimesPerStop = feed.StopTimes.GroupBy(x => x.StopId).ToDictionary(x => x.Key, v => v.ToList());
        var stopTimesPerTrip = feed.StopTimes.OrderBy(x => x.StopSequence).GroupBy(x => x.TripId).ToDictionary(x => x.Key, v => v.ToList());
        var transfersFromStop = feed.Transfers.GroupBy(x => x.FromStopId).ToDictionary(x => x.Key, v => v.ToList());
        var stops = feed.Stops.ToDictionary(x => x.Id);
        var routePerTrip = feed.Trips.ToDictionary(x => x.Id, v => v.RouteId);
        routePerTrip.Add(string.Empty, string.Empty);

        return new GTFSData(stopTimesPerStop, stopTimesPerTrip, transfersFromStop, stops);
    }
}