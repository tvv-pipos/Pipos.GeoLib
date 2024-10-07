using Pipos.GeoLib.NetworkUtilities.Model;

namespace Pipos.GeoLib.NetworkUtilities.Processing;

public class GTFSRaider
{
    public Dictionary<long, int> CalculateDistances(GTFSData data, GTFSStop stop, int maxTravelTimeInSeconds, 
        int startTime, bool removeInitialWaitTime)
    {
        var isochroneStops = new Dictionary<long, int>();
        if (stop != null)
        {
            var visitedStops = new HashSet<long>();

            var queue = new MinHeap<Departure>();
            queue.Add(new Departure(stop.Id, 10, 0, 0, null));

            while (queue.Count > 0)
            {
                var currentStopInfo = queue.RemoveMin();
                var currentStopId = currentStopInfo.TargetStopId;
                int currentTravelTime = currentStopInfo.TravelTime;

                if (visitedStops.Contains(currentStopId))
                {
                    continue;
                }

                visitedStops.Add(currentStopId);

                if (currentTravelTime <= maxTravelTimeInSeconds)
                {
                    isochroneStops[currentStopId] = currentTravelTime * 1000;
                }

                // Vi har nått en målpunkt, gå vidare.
                if (data.GetStop(currentStopId).StopType == StopType.Target)
                {
                    isochroneStops[currentStopId] -= (GetInitialWaitTime(currentStopInfo) * 1000);
                    continue;
                }

                // Hämta alla avgångar från den aktuella hållplatsen
                var departures = GetDepartures(data, data.GetStop(currentStopId), startTime + currentTravelTime, 
                    3600, removeInitialWaitTime);

                foreach (var departue in departures)
                {
                    // Beräkna ankomsttiden vid nästa hållplats
                    int nextTravelTime = currentTravelTime + departue.TravelTime;

                    int transferCount = currentStopInfo.TransferCount;
                    if (currentStopInfo.TripId > 0 &&
                        currentStopInfo.TripId != departue.TripId)
                    {
                        transferCount++;
                        //nextTravelTime += 5 * 60;
                    }

                    // Hämta nästa hållplats
                    var nextStopId = departue.TargetStopId;

                    // Om nästa hållplats inte har besökts än och den inte är längre bort än maxTravelTimeInSeconds
                    if (!visitedStops.Contains(nextStopId) && nextTravelTime <= maxTravelTimeInSeconds)
                    {
                        // Lägg till nästa hållplats i kön för att utforska den
                        queue.Add(new Departure(nextStopId, nextTravelTime, departue.TripId, transferCount, 
                            currentStopInfo, departue.InitalWaitTime));
                    }
                }
            }
        }
        return isochroneStops;
    }

    static List<Departure> GetDepartures(GTFSData data, GTFSStop stop, int currentTime, int maxWaitTime, bool removeInitialWaitTime = false)
    {
        var result = new List<Departure>();

        var transferStopTimes = data.GetTransfersFromStop(stop.Id).SelectMany(t =>
            data.GetStopTimesFromStop(t.ToStopId, 
                currentTime + t.MinimumTransferTime, 
                currentTime + t.MinimumTransferTime + maxWaitTime)
        );

        var customEndpoints = data.GetTransfersFromStop(stop.Id)
            .Where(t => data.GetStop(t.ToStopId).StopType == StopType.Target)
            .Select(t => new Departure(t.ToStopId, t.MinimumTransferTime, 0, 0, null));

        result.AddRange(customEndpoints);

        var regularStopTimes = data.GetStopTimesFromStop(stop.Id, currentTime, currentTime + maxWaitTime);
        var validStopTimes = transferStopTimes.Concat(regularStopTimes);

        foreach (var stopTime in validStopTimes)
        {
            var (travelTime, nextStopId) = GetHeadTravelTime(data, stopTime);
            travelTime += (stopTime.DepartureTime - currentTime);

            var initTime = 0;
            // Drar bort den initiala väntetiden, dvs resenären anpassar tiden den går hemifrån mot avgången.
            if (removeInitialWaitTime && stop.StopType == StopType.Start)
            {
                var transfer = data.GetTransfersFromStop(stop.Id).First(x => x.ToStopId == stopTime.StopId);
                initTime = travelTime - transfer.MinimumTransferTime;
            }

            if (nextStopId > 0)
            {
                result.Add(new Departure(nextStopId, travelTime, stopTime.TripId, 0, null, initTime));
            }
        }

        return result;
    }

    static int GetInitialWaitTime(Departure departure)
    {
        Departure? dep = departure;
        while (dep != null)
        {
            if (dep.InitalWaitTime > 0)
            {
                return dep.InitalWaitTime;
            }
            dep = dep.Previous;
        }
        return 0;
    }

    static (int travelTime, long nextStopId) GetHeadTravelTime(
        GTFSData data, GTFSStopTime stopTime)
    {
        var sequence = data.GetStopTimesFromTrip(stopTime.TripId);

        if (stopTime.StopSequence == sequence.Count)
        {
            return (0, 0);
        }

        var nextStopTime = sequence[(int)stopTime.StopSequence];
        var totalTime = nextStopTime.ArrivalTime - stopTime.ArrivalTime;
        return (totalTime, nextStopTime.StopId);
    }

    internal class Departure : IComparable<Departure>
    {
        public long TripId { get; set; }
        public long TargetStopId { get; set; }
        public int TravelTime { get; set; }
        public int TransferCount { get; set; }
        public int InitalWaitTime { get; set; }
        public Departure? Previous { get; set; }

        public Departure(long targetStopId, int travelTime, long tripId,
            int transferCount, Departure? prev, int initialWaitTime = 0)
        {
            TargetStopId = targetStopId;
            TravelTime = travelTime;
            TripId = tripId;
            TransferCount = transferCount;
            Previous = prev;
            InitalWaitTime = initialWaitTime;
        }

        public int CompareTo(Departure? other)
        {
            return TravelTime.CompareTo(other?.TravelTime);
        }

        public override string ToString()
        {
            return "[" + TargetStopId + " | " + TravelTime + " | " + TripId + "]";
        }
    }
}