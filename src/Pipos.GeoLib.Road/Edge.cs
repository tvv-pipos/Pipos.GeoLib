using NetTopologySuite.Geometries;
using Pipos.GeoLib.Core.Model;
using System.Linq;

namespace Pipos.GeoLib.Road;

public class Edge
{
    public uint Id { get; set; }
    public Node Source { get; set; }
    public Node Target { get; set; }
    public float Distance { get; set; }
    public byte ForwardSpeed { get; set; }
    public byte BackwardSpeed { get; set; }
    public YearSet Years { get; set; }
    public Attribute Attribute { get; set; }
    public Segment[] Segments { get; set; }

    public Edge(uint id, Node source, Node target, float distance, byte forwardSpeed, byte backwardSpeed, float[] segments, Attribute attribute, YearSet yearSet) 
    {
        Id = id;
        Source = source;
        Target = target;
        Distance = distance;
        ForwardSpeed = forwardSpeed;
        BackwardSpeed = backwardSpeed;
        Years = new YearSet(yearSet);
        Attribute = attribute;

        Int32 seglen = segments.Length / 4;
        Segments = new Segment[seglen];
        for(Int32 i = 0; i < seglen; i++)
        {
            Segments[i] = new Segment(segments[i * 4 + 0], segments[i * 4 + 1], segments[i * 4 + 2], segments[i * 4 + 3], this);
        }
    }
    public override bool Equals(Object? obj)
    {
        if(obj == null || !(obj is Edge))
            return false;
        Edge edge = (Edge)obj;
        
        return Id == edge.Id;
    }
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
    public bool IsSame(Edge edge)
    {
        
        return (Source.Id == edge.Source.Id && Target.Id == edge.Target.Id && Distance == edge.Distance && 
               ForwardSpeed == edge.ForwardSpeed && BackwardSpeed == edge.BackwardSpeed && Attribute.Value == edge.Attribute.Value) || 
               (Source.Id == edge.Target.Id && Target.Id == edge.Source.Id && Distance == edge.Distance && 
               ForwardSpeed == edge.BackwardSpeed && BackwardSpeed == edge.ForwardSpeed && Attribute.Value == edge.Attribute.Reverse().Value);
    }

    public Node GetOther(Node node)
    {
        if(Source == node)
            return Target;
        return Source;
    }
    
    public void AddSegment(List<Coordinate> line, Node start)
    {
        if(Source == start)
        {
            for(uint i = 0; i < Segments.Length; i++)
            {
                Segment segment = Segments[i];
                line.Add(new Coordinate(segment.X2, segment.Y2));
            }
        }
        else
        {
            for(int i = Segments.Length; i > 0; i--)
            {
                Segment segment = Segments[i - 1];
                line.Add(new Coordinate(segment.X1, segment.Y1));
            }
        }
    }

    public void AddSegmentsAfter(Edge edge)
    {
        Segments = Segments.Concat(edge.Segments).ToArray();
    }

    public void AddSegmentsAfterReveresed(Edge edge)
    {
        Segments = Segments.Concat(edge.Segments.Reverse()).ToArray();
    }

    public void AddSegmentsBeforeReversed(Edge edge)
    {
        Segments = edge.Segments.Reverse().Concat(Segments).ToArray();
    }
}