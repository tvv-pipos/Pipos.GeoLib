namespace Pipos.GeoLib.NetworkUtilities.Model;

public class GTFSData
{
    private Dictionary<long, List<GTFSStopTime>> _stopTimesPerStop;
    private Dictionary<long, List<GTFSStopTime>> _stopTimesPerTrip;
    private Dictionary<long, List<GTFSTransfer>> _transfersFromStop;
    private Dictionary<long, GTFSStop> _stops;

    public GTFSData(Dictionary<long, List<GTFSStopTime>> stopTimesPerStop, Dictionary<long, List<GTFSStopTime>> stopTimesPerTrip,
        Dictionary<long, List<GTFSTransfer>> transfersFromStop, Dictionary<long, GTFSStop> stops)
    {
        _stops = stops;
        _stopTimesPerStop = stopTimesPerStop;
        _stopTimesPerTrip = stopTimesPerTrip;
        _transfersFromStop = transfersFromStop;
    }

    public Dictionary<long, GTFSStop> Stops => _stops;
    public Dictionary<long, List<GTFSStopTime>> StopTimesPerStop => _stopTimesPerStop;
    public Dictionary<long, List<GTFSStopTime>> StopTimesPerTrip => _stopTimesPerTrip;
    public Dictionary<long, List<GTFSTransfer>> TransfersFromStop => _transfersFromStop;

    public GTFSStop GetStop(long stopId) => _stops[stopId];
    public IEnumerable<GTFSTransfer> GetTransfersFromStop(long stopId) => _transfersFromStop[stopId];
    public IEnumerable<GTFSStopTime> GetStopTimesFromStop(long stopId, int fromTime, int toTime)
    {
        var start = 0;
        int end;
        var stopTimes = GetStopTimesFromStop(stopId);
        int dep;

        for (var i = 0; i < stopTimes.Count; i++)
        {
            dep = stopTimes[i].DepartureTime;
            if (start == 0 && dep >= fromTime)
            {
                start = i;
            }

            if (start > 0 && dep > toTime)
            {
                end = i;
                return stopTimes.GetRange(start, Math.Max(end - start, 1));
            }
        }
        return new GTFSStopTime[0];
    }
    public List<GTFSStopTime> GetStopTimesFromStop(long stopId) => _stopTimesPerStop.TryGetValue(stopId, out var stopTimes) ? 
        stopTimes : new List<GTFSStopTime>();
    public List<GTFSStopTime> GetStopTimesFromTrip(long tripId) => _stopTimesPerTrip[tripId];

    public void AddStop(GTFSStop stop)
    {
        _stops[stop.Id] = stop;
        if (!_stopTimesPerStop.ContainsKey(stop.Id))
        {
            _stopTimesPerStop[stop.Id] = new List<GTFSStopTime>();
        }
    } 

    public void AddTransfer(GTFSTransfer transfer)
    {
        if (_transfersFromStop.TryGetValue(transfer.FromStopId, out var list))
        {
            list.Add(transfer);
        }
        else
        {
            _transfersFromStop[transfer.FromStopId] = new List<GTFSTransfer>{ transfer };
        }
    }
}