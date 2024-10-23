using NetTopologySuite.Geometries;
using NetTopologySuite.Index.HPRtree;
using Pipos.GeoLib.Core.Api;
using Pipos.GeoLib.Core.Model;

namespace Pipos.GeoLib.Road;

public class ConnectionIndexBuilder
{
    private HPRtree<Segment> RTree;

    public ConnectionIndexBuilder(uint length) 
    {
        RTree = new HPRtree<Segment>((int)length);
    }
    public ConnectionIndexBuilder AddSegment(Segment segment)
    {
        RTree.Insert(segment.GetEnvelope(), segment);
        return this;
    }
    public ConnectionIndexBuilder AddSegments(List<Segment> segments)
    {
        foreach(Segment segment in segments)
        {
            RTree.Insert(segment.GetEnvelope(), segment);
        }
        return this;
    }

    public ConnectionIndex Build()
    {
        RTree.Build();
        return new ConnectionIndex(RTree);
    }
}

public class ConnectionIndex : IConnectionIndex
{
    public HPRtree<Segment> RTree { get; private set; }

    public ConnectionIndex(HPRtree<Segment> rtree)
    {
        RTree = rtree;
    }

    public IConnection Point(float x, float y, float radius, Year year, IConnectionRule rule)
    {
        if(rule is not ConnectionRule)
            return new Connection();
        
        var connectionRule = (ConnectionRule)rule;
        Attribute noAttr = new Attribute
        {
            Class = 0,
            Ferry = connectionRule.NoFerry,
            ForwardProhibited = connectionRule.NoOneWay,
            BackwardProhibited = connectionRule.NoOneWay,
            Motorway = connectionRule.NoMotorway,
            DisconnectedIsland = connectionRule.NoDisconnectedIsland
        };

        Envelope searchEnvelope = new Envelope(x - radius, x + radius, y - radius, y + radius);
        IList<Segment> segements = RTree.Query(searchEnvelope);
        float distance = float.MaxValue;
        float px = 0, py = 0;
        Segment? segment = null;
        foreach(Segment s in segements)
        {
            if(s.Edge.Years.HasYear(year) && ((noAttr.Value & s.Edge.Attribute.Value) == 0) 
                && (s.Edge.Attribute.DisconnectedIsland || !connectionRule.OnlyDisconnectedIsland))
            {
                float tmp_distance, tmp_x, tmp_y;
                s.DistanceAndPosFromPoint(x, y, out tmp_distance, out tmp_x, out tmp_y);
                if(distance > tmp_distance)
                {
                    distance = tmp_distance;
                    px = tmp_x;
                    py = tmp_y;
                    segment = s;
                }
            }
        }
        /* Calculate source and target distance */
        if(segment != null)
        {
            bool sourceCalculated = false;
            float linestringDistance = 0.0f;
            float linestringSource = 0.0f;
            float dx, dy;
            ConnectionPoint connection = new ConnectionPoint();

            foreach(Segment s in segment.Edge.Segments)
            {
                if(segment == s)
                {
                    dx = px - s.X1;
                    dy = py - s.Y1;
                    linestringSource += MathF.Sqrt(dx * dx + dy * dy);
                    sourceCalculated = true;
                }
                dx = s.X2 - s.X1;
                dy = s.Y2 - s.Y1;
                linestringDistance += MathF.Sqrt(dx * dx + dy * dy);
                if(!sourceCalculated)
                    linestringSource = linestringDistance;
            }
            connection.Segment = segment;
            connection.SourceDistance = (linestringSource / linestringDistance) * connection.Segment.Edge.Distance;
            connection.TargetDistance = connection.Segment.Edge.Distance - connection.SourceDistance;
            connection.X = px;
            connection.Y = py;
            connection.SearchX = x;
            connection.SearchY = y;
            return new Connection{connection};
        }
        return new Connection();
    }
    public IConnection Point(Point point, float radius, Year year, IConnectionRule rule)
    {
        return Point((float)point.X, (float)point.Y, radius, year, rule);
    }

    public List<IConnection> Points(List<Point> points, float radius, Year year, IConnectionRule rule)
    {
        List<IConnection> connections = new List<IConnection>(points.Count);
        for(int i = 0; i < points.Count; i++)
        {
            connections.Add(Point((float)points[i].X, (float)points[i].Y, radius, year, rule));
        }
        return connections;
    }

    public IConnection PiposId(uint piposId, float radius, Year year, IConnectionRule rule)
    {
        // Connect center point
        // TODO: multi connect ...
        return Point(PiposID.X(piposId) + 125.0f, PiposID.Y(piposId) + 125.0f, radius, year, rule);
    }

    public List<IConnection> PiposIds(List<uint> piposIds, float radius, Year year, IConnectionRule rule)
    {
        List<IConnection> connections = new List<IConnection>(piposIds.Count);
        for(int i = 0; i < piposIds.Count; i++)
        {
            connections.Add(PiposId(piposIds[i], radius, year, rule));
        }
        return connections;    
    }
}
