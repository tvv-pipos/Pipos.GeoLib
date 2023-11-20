using GTFS.Entities;
using Pipos.Common.NetworkUtilities.Model;

namespace Pipos.Common.NetworkUtilities.Processing;

public class GTFSRaider
{
    public Dictionary<string, int> CalculateDistances(GTFSData data, Stop stop, int maxTravelTimeInSeconds, 
        int startTime, bool removeInitialWaitTime)
    {
        var isochroneStops = new Dictionary<string, int>();
        if (stop != null)
        {
            var visitedStops = new HashSet<string>();

            var queue = new MinHeap<Departure>();
            queue.Add(new Departure(stop.Id, 0, "", 0, null));

            while (queue.Count > 0)
            {
                var currentStopInfo = queue.RemoveMin();
                string currentStopId = currentStopInfo.TargetStopId;
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
                if (data.GetStop(currentStopId).Tag != null && StopType.Target == (StopType)data.GetStop(currentStopId).Tag)
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
                    if (!string.IsNullOrEmpty(currentStopInfo.TripId) &&
                        currentStopInfo.TripId != departue.TripId)
                    {
                        transferCount++;
                        //nextTravelTime += 5 * 60;
                    }

                    // Hämta nästa hållplats
                    string nextStopId = departue.TargetStopId;

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

    static List<Departure> GetDepartures(GTFSData data, Stop stop, int currentTime, int maxWaitTime, bool removeInitialWaitTime = false)
    {
        var result = new List<Departure>();

        var transferStopTimes = data.GetTransfersFromStop(stop.Id).SelectMany(t =>
            data.GetStopTimesFromStop(t.ToStopId)
                .Where(x => x.DepartureTime?.TotalSeconds >= currentTime + int.Parse(t.MinimumTransferTime) &&
                    x.DepartureTime?.TotalSeconds < currentTime + int.Parse(t.MinimumTransferTime) + maxWaitTime));

        var customEndpoints = data.GetTransfersFromStop(stop.Id)
            .Where(t => data.GetStop(t.ToStopId).Tag != null && StopType.Target == (StopType)data.GetStop(t.ToStopId).Tag)
            .Select(t => new Departure(t.ToStopId, int.Parse(t.MinimumTransferTime), "", 0, null));

        result.AddRange(customEndpoints);

        var regularStopTimes = data.GetStopTimesFromStop(stop.Id)
            .Where(x => x.DepartureTime?.TotalSeconds >= currentTime &&
                x.DepartureTime?.TotalSeconds < currentTime + maxWaitTime);

        var validStopTimes = transferStopTimes.Concat(regularStopTimes);

        foreach (var stopTime in validStopTimes)
        {
            var (travelTime, nextStopId) = GetHeadTravelTime(data, stopTime);
            travelTime += (stopTime.DepartureTime?.TotalSeconds - currentTime) ?? 0;

            var initTime = 0;
            // Drar bort den initiala väntetiden, dvs resenären anpassar tiden den går hemifrån mot avgången.
            if (removeInitialWaitTime && stop.Tag != null && ((StopType)stop.Tag) == StopType.Start)
            {
                var transfer = data.GetTransfersFromStop(stop.Id).First(x => x.ToStopId == stopTime.StopId);
                initTime = travelTime - int.Parse(transfer.MinimumTransferTime);
            }

            if (!string.IsNullOrEmpty(nextStopId))
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

    static (int travelTime, string nextStopId) GetHeadTravelTime(
        GTFSData data, StopTime stopTime)
    {
        var sequence = data.GetStopTimesFromTrip(stopTime.TripId);

        if (stopTime.StopSequence == sequence.Count)
        {
            return (0, string.Empty);
        }

        var nextStopTime = sequence[(int)stopTime.StopSequence];
        var totalTime = (nextStopTime.ArrivalTime?.TotalSeconds - stopTime.ArrivalTime?.TotalSeconds) ?? 0;
        return (totalTime, nextStopTime.StopId);
    }

    public enum StopType
    {
        Start,
        Target
    }

    internal class Departure : IComparable<Departure>
    {
        public string TripId { get; set; } = string.Empty;
        public string TargetStopId { get; set; } = string.Empty;
        public int TravelTime { get; set; }
        public int TransferCount { get; set; }
        public int InitalWaitTime { get; set; }
        public Departure? Previous { get; set; }

        public Departure(string targetStopId, int travelTime, string tripId,
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