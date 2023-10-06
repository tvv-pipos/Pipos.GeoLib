namespace RoutingKit;

public class ContractionHierarchyExtraInfo
{
    public class Side
    {
        public List<int> MidNode { get; set; } = new List<int>();
        public List<int> Tail { get; set; } = new List<int>();
    }
    public Side Forward { get; set; } = new Side();
    public Side Backward { get; set; } = new Side()!;
}

