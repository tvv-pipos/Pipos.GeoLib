namespace Pipos.GeoLib.NetworkUtilities.Model;

public enum NodeType : ushort
{
    Connection,
    Default,
}

public class Node
{
    public Node(int[] coordinate, int networkGroup, int functionClass, NodeType nodeType = NodeType.Default)
    {
        X = coordinate[0];
        Y = coordinate[1];
        NodeType = nodeType;
        NetworkGroup = networkGroup;
        FunctionClass = functionClass;
        Edges = new List<Edge>();
        Pinned = false;
    }

    public Node(int x, int y, NodeType nodeType = NodeType.Default)
    {
        X = x;
        Y = y;
        NodeType = nodeType;
        Edges = new List<Edge>();
        Pinned = false;
    }

    public Node()
    {
        NodeType = NodeType.Default;
        Edges = new List<Edge>();
        Pinned = false;
    }

    public long Id => (0xFFFFFFFF & Y) << 32 | (0xFFFFFFFF & X);
    public int Index { get; set; }
    public int Idx { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int FunctionClass { get; set; }
    public int NetworkGroup { get; set; }
    public bool Pinned { get; set; }
    public List<Edge> Edges { get; }
    public NodeType NodeType { get; set; }

    public int DistanceTo(Node other)
    {
        double dx = X - other.X;
        double dy = Y - other.Y;
        return (int)Math.Round(Math.Sqrt(dx * dx + dy * dy));
    }

    public int DistanceTo(int x, int y)
    {
        double dx = X - x;
        double dy = Y - y;
        return (int)Math.Round(Math.Sqrt(dx * dx + dy * dy));
    }

    public override string ToString()
    {
        return $"Index: {Index} Edges: {Edges.Count}";
    }
}