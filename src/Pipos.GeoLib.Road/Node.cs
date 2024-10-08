namespace Pipos.GeoLib.Road;

public class Node
{
    public uint Id { get; set; }
    public List<Edge> Edges { get; set; }
    public Node(uint id)
    {
        Id = id;
        Edges = new List<Edge>();
    }
}