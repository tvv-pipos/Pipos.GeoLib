using System.Diagnostics; 
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
    public override bool Equals(Object? obj)
    {
        if(obj == null || !(obj is Node))
            return false;
        Node node = (Node)obj;
        
        return Id == node.Id;
    }
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
    public void ReplaceEdge(Edge oldEdge, Edge newEdge)
    {
        Debug.Assert(Edges.Count > 0);
        bool found = false;
        for(int i = 0; i < Edges.Count; i++)
        {                
            if(Edges[i] == oldEdge)
            {
                Edges[i] = newEdge;
                found = true;
            }
        }
        Debug.Assert(found);
    }
    public bool RemoveEdge(Edge edge)
    {
        return Edges.Remove(edge);
    }

    
}