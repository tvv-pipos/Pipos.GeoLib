namespace Pipos.GeoLib.NetworkUtilities.Model;

public class RoutingGraph
{
        public int[] FirstOut { get; set; } = null!;
        public List<int> Head { get; set; } = new List<int>();
        public List<int> Tail { get; set; } = new List<int>();
        public List<int> TravelTime { get; set; } = new List<int>();
        public List<int> GeoDistance { get; set; } = new List<int>();
        public List<int> Latitude { get; set; } = new List<int>();
        public List<int> Longitude { get; set; } = new List<int>();
        public List<int> ForbiddenTurnFromArc { get; set; } = new List<int>();
        public List<int> ForbiddenTurnToArc { get; set; } = new List<int>();

        public int NodeCount => Latitude.Count;
        public int ArcCount => Head.Count;
}