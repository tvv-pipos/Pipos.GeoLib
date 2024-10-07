namespace Pipos.GeoLib.NetworkUtilities.Model;

public class GTFSStop
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public StopType StopType { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
}

public class GTFSStopTime
{
    public long StopId { get; set; }
    public long TripId { get; set; }
    public int ArrivalTime { get; set; }
    public int DepartureTime { get; set; }
    public int StopSequence { get; set; }
} 

public class GTFSTransfer
{
    public long FromStopId { get; set; }
    public long ToStopId { get; set; }
    public int MinimumTransferTime { get; set; }
}

public enum StopType
{
    Default,
    Start,
    Target
}