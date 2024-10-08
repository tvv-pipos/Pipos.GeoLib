using NetTopologySuite.Geometries;
using Pipos.GeoLib.Core.Api;

namespace Pipos.GeoLib.Road;

public class ConnectionPoint
{
    public Segment? Segment { get; set; }
    public float SourceDistance { get; set; }
    public float TargetDistance { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float SearchX { get; set; }
    public float SearchY { get; set; }

    public ConnectionPoint() 
    {
        Segment = null;
        SourceDistance = 0;
        TargetDistance = 0;
        X = 0;
        Y = 0;
        SearchX = 0;
        SearchY = 0;
    }

    public bool IsConnected()
    {
        return Segment != null;
    }
    public float GetConnectionDistance()
    {
        float dx = X - SearchX;
        float dy = Y - SearchY;
        return MathF.Sqrt(dx*dx + dy*dy);
    }   
    public void AddStartSegment(List<Coordinate> line, Node start)
    {
        if(Segment == null)
            return;

        Edge edge = Segment.Edge;

        if(edge.Source == start)
        {
            for(uint i = 0; i < edge.Segments.Length; i++)
            {
                Segment segment = edge.Segments[i];
                if(segment == Segment)
                {
                    line.Add(new Coordinate(X, Y));
                    break;
                }
                else
                {
                    line.Add(new Coordinate(segment.X2, segment.Y2));
                }
            }
        }
        else
        {
            for(int i = edge.Segments.Length; i > 0; i--)
            {
                Segment segment = edge.Segments[i - 1];
                if(segment == Segment)
                {
                    line.Add(new Coordinate(X, Y));
                    break;
                }
                else
                {
                    line.Add(new Coordinate(segment.X1, segment.Y1));
                }
            }
        }
        
    }
    
    public void AddEndSegment(List<Coordinate> line, Node start)
    {
        if(Segment == null)
            return;

        Edge edge = Segment.Edge;

        if(edge.Target == start)
        {
            bool found = false;
            for(uint i = 0; i < edge.Segments.Length; i++)
            {
                Segment segment = edge.Segments[i];
                if(segment == Segment)
                {
                    line.Add(new Coordinate(X, Y));
                    found = true;
                }
                if(found)
                {
                    line.Add(new Coordinate(segment.X2, segment.Y2));
                }
            }
        }
        else
        {
            bool found = false;
            for(int i = edge.Segments.Length; i > 0; i--)
            {
                Segment segment = edge.Segments[i - 1];
                if(segment == Segment)
                {
                    line.Add(new Coordinate(X, Y));
                    found = true;
                }
                if(found)
                {
                    line.Add(new Coordinate(segment.X1, segment.Y1));
                }
            }
        }
    }
}