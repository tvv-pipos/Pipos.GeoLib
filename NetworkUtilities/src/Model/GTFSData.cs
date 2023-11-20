using GTFS.Entities;
using GTFS.Filters;

namespace Pipos.Common.NetworkUtilities.Model;

public class GTFSData
{
    private Dictionary<string, List<StopTime>> _stopTimesPerStop;
    private Dictionary<string, List<StopTime>> _stopTimesPerTrip;
    private Dictionary<string, List<Transfer>> _transfersFromStop;
    private Dictionary<string, Stop> _stops;

    public GTFSData(Dictionary<string, List<StopTime>> stopTimesPerStop, Dictionary<string, List<StopTime>> stopTimesPerTrip,
        Dictionary<string, List<Transfer>> transfersFromStop, Dictionary<string, Stop> stops)
    {
        _stops = stops;
        _stopTimesPerStop = stopTimesPerStop;
        _stopTimesPerTrip = stopTimesPerTrip;
        _transfersFromStop = transfersFromStop;
    }

    public Dictionary<string, Stop> Stops => _stops;
    public Dictionary<string, List<StopTime>> StopTimesPerStop => _stopTimesPerStop;
    public Dictionary<string, List<StopTime>> StopTimesPerTrip => _stopTimesPerTrip;
    public Dictionary<string, List<Transfer>> TransfersFromStop => _transfersFromStop;

    public Stop GetStop(string stopId) => _stops[stopId];
    public List<Transfer> GetTransfersFromStop(string stopId) => _transfersFromStop[stopId];
    public List<StopTime> GetStopTimesFromStop(string stopId) => _stopTimesPerStop.TryGetValue(stopId, out var stopTimes) ? 
        stopTimes : new List<StopTime>();
    public List<StopTime> GetStopTimesFromTrip(string tripId) => _stopTimesPerTrip[tripId];

    public void AddStop(Stop stop)
    {
        _stops[stop.Id] = stop;
        if (!_stopTimesPerStop.ContainsKey(stop.Id))
        {
            _stopTimesPerStop[stop.Id] = new List<StopTime>();
        }
    } 

    public void AddTransfer(Transfer transfer)
    {
        if (_transfersFromStop.TryGetValue(transfer.FromStopId, out var list))
        {
            list.Add(transfer);
        }
        else
        {
            _transfersFromStop[transfer.FromStopId] = new List<Transfer>{ transfer };
        }
    }
}