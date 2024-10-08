using NetTopologySuite.Geometries;
using Pipos.GeoLib.Core.Api;
using Pipos.GeoLib.Core.Model;
using Pipos.GeoLib.Road.Model;

namespace Pipos.GeoLib.Road.Dijkstra;

internal static class NetworkUtils
{
    internal static readonly float TimeUnitConversion = 3.6f;
     
    internal class SingleEdgeResult
    {
        public bool Found { get; set; }
        public float Distance { get; set; }
        public float Time { get; set; }
    }

    internal static SingleEdgeResult FindSingleEdgeDistance(Connection start, Connection end, QueryOptions options)
    {
        SingleEdgeResult result = new SingleEdgeResult{Found = false};
        foreach(ConnectionPoint startP in start)
        {
            foreach(ConnectionPoint endP in end)
            {
                if(startP.Segment != null && endP.Segment != null)
                {
                    Edge start_edge = startP.Segment.Edge;
                    Edge end_edge = endP.Segment.Edge;

                    if(start_edge == end_edge)
                    {
                        var tmp = ShortestDistanceTimeSingleEdgePoints(startP, endP, start_edge, options);
                        if(tmp.Found && (!result.Found || (result.Found && result.Distance > tmp.Distance)))
                            result = tmp;
                    }
                }
            }
        }
        return result;
    } 

    internal static SingleEdgeResult FindSingleEdgeTime(Connection start, Connection end, QueryOptions options)
    {
        SingleEdgeResult result = new SingleEdgeResult{Found = false};
        foreach(ConnectionPoint startP in start)
        {
            foreach(ConnectionPoint endP in end)
            {
                if(startP.Segment != null && endP.Segment != null)
                {
                    Edge start_edge = startP.Segment.Edge;
                    Edge end_edge = endP.Segment.Edge;

                    if(start_edge == end_edge)
                    {
                        var tmp = ShortestDistanceTimeSingleEdgePoints(startP, endP, start_edge, options);
                        if(tmp.Found && (!result.Found || (result.Found && result.Time > tmp.Time)))
                            result = tmp;
                    }
                }
            }
        }
        return result;
    } 


    internal static LineString? FindSingleEdgeDistanceLineString(Connection start, Connection end, QueryOptions options)
    {
        SingleEdgeResult result = new SingleEdgeResult{Found = false};
        LineString? ls = null;
        foreach(ConnectionPoint startP in start)
        {
            foreach(ConnectionPoint endP in end)
            {
                if(startP.Segment != null && endP.Segment != null)
                {
                    Edge start_edge = startP.Segment.Edge;
                    Edge end_edge = endP.Segment.Edge;

                    if(start_edge == end_edge)
                    {
                        var tmp = ShortestDistanceTimeSingleEdgePoints(startP, endP, start_edge, options);
                        if(tmp.Found && (!result.Found || (result.Found && result.Distance > tmp.Distance)))
                        {
                            result = tmp;
                            if(options.IncludeConnectionDistance)
                            {
                                ls = new LineString([new Coordinate(startP.SearchX, startP.SearchY), new Coordinate(startP.X, startP.Y), new Coordinate(endP.X, endP.Y), new Coordinate(endP.SearchX, endP.SearchY)]);
                            }
                            else
                            {
                                ls = new LineString([new Coordinate(startP.X, startP.Y), new Coordinate(endP.X, endP.Y)]);
                            }
                        }
                    }
                }
            }
        }
        return ls;
    } 
    internal static LineString? FindSingleEdgeTimeLineString(Connection start, Connection end, QueryOptions options)
    {
        SingleEdgeResult result = new SingleEdgeResult{Found = false};
        LineString? ls = null;
        foreach(ConnectionPoint startP in start)
        {
            foreach(ConnectionPoint endP in end)
            {
                if(startP.Segment != null && endP.Segment != null)
                {
                    Edge start_edge = startP.Segment.Edge;
                    Edge end_edge = endP.Segment.Edge;

                    if(start_edge == end_edge)
                    {
                        var tmp = ShortestDistanceTimeSingleEdgePoints(startP, endP, start_edge, options);
                        if(tmp.Found && (!result.Found || (result.Found && result.Distance > tmp.Distance)))
                        {
                            result = tmp;
                            if(options.IncludeConnectionDistance)
                            {
                                ls = new LineString([new Coordinate(startP.SearchX, startP.SearchY), new Coordinate(startP.X, startP.Y), new Coordinate(endP.X, endP.Y), new Coordinate(endP.SearchX, endP.SearchY)]);
                            }
                            else
                            {
                                ls = new LineString([new Coordinate(startP.X, startP.Y), new Coordinate(endP.X, endP.Y)]);
                            }
                        }
                    }
                }
            }
        }
        return ls;
    } 

    internal static bool StartWeightsDistance(Connection start, Dictionary<uint, float> weights, PriorityQueue<Node, float> queue, QueryOptions options)
    {
        bool found = false;
        foreach(ConnectionPoint startP in start)
        {
            float connectionDistance = 0.0f;

            if(options.IncludeConnectionDistance)
            {
                connectionDistance = startP.GetConnectionDistance();
            }

            if(startP.Segment != null)
            {
                var edge = startP.Segment.Edge;
                if(edge.BackwardSpeed > 0.0f)
                {
                    weights[edge.Source.Id] = startP.SourceDistance + connectionDistance;
                    queue.Enqueue(edge.Source, weights[edge.Source.Id]);
                    found = true;
                }

                if(edge.ForwardSpeed > 0.0f)
                {
                    weights[edge.Target.Id] = startP.TargetDistance + connectionDistance;
                    queue.Enqueue(edge.Target, weights[edge.Target.Id]);
                    found = true;
                }
            }
        }
        return found;
    }
    internal static bool StartWeightsTime(Connection start, Dictionary<uint, float> weights, PriorityQueue<Node, float> queue, QueryOptions options)
    {
        bool found = false;
        foreach(ConnectionPoint startP in start)
        {
            float connectionTime = 0.0f;
            
            if(options.IncludeConnectionDistance)
            {
                connectionTime = TimeUnitConversion * startP.GetConnectionDistance() / options.ConnectionSpeed;
            }

            if(startP.Segment != null)
            {
                var edge = startP.Segment.Edge;
                if(edge.BackwardSpeed > 0.0f)
                {
                    weights[edge.Source.Id] = (TimeUnitConversion * startP.SourceDistance / (float)edge.BackwardSpeed) + connectionTime;
                    queue.Enqueue(edge.Source, weights[edge.Source.Id]);
                    found = true;
                }

                if(edge.ForwardSpeed > 0.0f)
                {
                    weights[edge.Target.Id] = (TimeUnitConversion * startP.TargetDistance / (float)edge.ForwardSpeed) + connectionTime;
                    queue.Enqueue(edge.Target, weights[edge.Target.Id]);
                    found = true;
                }
            }
        }
        return found;
    }

    internal static bool StartWeightsDistanceWithTime(Connection start, Dictionary<uint, TimeDistanceResult> weights, PriorityQueue<Node, float> queue, QueryOptions options)
    {
        bool found = false;
        foreach(ConnectionPoint startP in start)
        {
            float connectionDistance = 0.0f;
            float connectionTime = 0.0f;

            if(options.IncludeConnectionDistance)
            {
                connectionDistance = startP.GetConnectionDistance();
                connectionTime = TimeUnitConversion * connectionDistance / options.ConnectionSpeed;
            }

            if(startP.Segment != null)
            {
                var edge = startP.Segment.Edge;
                if(edge.BackwardSpeed > 0.0f)
                {
                    float distance = startP.SourceDistance + connectionDistance;
                    float time = (TimeUnitConversion * startP.SourceDistance / (float)edge.BackwardSpeed) + connectionTime;
                    weights[edge.Source.Id] = new TimeDistanceResult{Distance = distance, Time = time};
                    queue.Enqueue(edge.Source, weights[edge.Source.Id].Distance);
                    found = true;
                }

                if(edge.ForwardSpeed > 0.0f)
                {
                    float distance = startP.TargetDistance + connectionDistance;
                    float time = (TimeUnitConversion * startP.TargetDistance / (float)edge.ForwardSpeed) + connectionTime;
                    weights[edge.Target.Id] = new TimeDistanceResult{Distance = distance, Time = time};
                    queue.Enqueue(edge.Target, weights[edge.Target.Id].Distance);
                    found = true;
                }
            }
        }
        return found;
    }

    internal static bool StartWeightsTimeWithDistance(Connection start, Dictionary<uint, TimeDistanceResult> weights, PriorityQueue<Node, float> queue, QueryOptions options)
    {
        bool found = false;
        foreach(ConnectionPoint startP in start)
        {
            float connectionDistance = 0.0f;
            float connectionTime = 0.0f;

            if(options.IncludeConnectionDistance)
            {
                connectionDistance = startP.GetConnectionDistance();
                connectionTime = TimeUnitConversion * connectionDistance / options.ConnectionSpeed;
            }

            if(startP.Segment != null)
            {
                var edge = startP.Segment.Edge;
                if(edge.BackwardSpeed > 0.0f)
                {
                    float distance = startP.SourceDistance + connectionDistance;
                    float time = (TimeUnitConversion * startP.SourceDistance / (float)edge.BackwardSpeed) + connectionTime;
                    weights[edge.Source.Id] = new TimeDistanceResult{Distance = distance, Time = time};
                    queue.Enqueue(edge.Source, weights[edge.Source.Id].Time);
                    found = true;
                }

                if(edge.ForwardSpeed > 0.0f)
                {
                    float distance = startP.TargetDistance + connectionDistance;
                    float time = (TimeUnitConversion * startP.TargetDistance / (float)edge.ForwardSpeed) + connectionTime;
                    weights[edge.Target.Id] = new TimeDistanceResult{Distance = distance, Time = time};
                    queue.Enqueue(edge.Target, weights[edge.Target.Id].Time);
                    found = true;
                }
            }
        }
        return found;
    }

    internal static SingleEdgeResult ShortestDistanceTimeSingleEdgePoints(ConnectionPoint start, ConnectionPoint end, Edge edge, QueryOptions options)
    {
        float searchDx = end.SearchX - start.SearchX;
        float searchDy = end.SearchY - start.SearchY;

        if(start.Segment != null && start.Segment == end.Segment)
        {
            float dx = end.X - start.X;
            float dy = end.Y - start.Y;
            float ds = (start.X - start.Segment.X1) * (start.X - start.Segment.X1) + (start.Y - start.Segment.Y1) * (start.Y - start.Segment.Y1);
            float de = (end.X - start.Segment.X1) * (end.X - start.Segment.X1) + (end.Y - start.Segment.Y1) * (end.Y - start.Segment.Y1);
            if(ds < de && edge.ForwardSpeed > 0)
            {
                var distance = MathF.Sqrt(dx * dx + dy * dy);
                var time = TimeUnitConversion * distance / edge.ForwardSpeed;

                if(options.IncludeConnectionDistance)
                {
                    var (cd, ct) = ConnectionDistanceTime(start, end, options.ConnectionSpeed);
                    distance += cd;
                    time += ct;
                }

                return new SingleEdgeResult{Found = true, Distance = distance, Time = time};
            }
            else if(ds > de && edge.BackwardSpeed > 0)
            {
                var distance = MathF.Sqrt(dx * dx + dy * dy);
                var time = TimeUnitConversion * distance / edge.BackwardSpeed;

                if(options.IncludeConnectionDistance)
                {
                    var (cd, ct) = ConnectionDistanceTime(start, end, options.ConnectionSpeed);
                    distance += cd;
                    time += ct;
                }

                return new SingleEdgeResult{Found = true, Distance = distance, Time = time};
            }
        }
        else 
        {
            bool forward = true;
            int start_seg_idx = 0, end_seg_idx = 0;
            var seg = edge.Segments;

            for(int i = 0; i < seg.Length; i++)
            {
                if(start.Segment == seg[i])
                {
                    forward = true;
                    start_seg_idx = i;
                    break;
                }
                if(end.Segment == seg[i])
                {
                    forward = false;
                    start_seg_idx = i;
                    break;
                }
            }

            for(int i = start_seg_idx + 1; i < seg.Length; i++)
            {
                if(start.Segment == seg[i])
                {
                    end_seg_idx = i;
                    break;
                }
                if(end.Segment == seg[i])
                {
                    end_seg_idx = i;
                    break;
                }
            }

            if(forward && edge.ForwardSpeed > 0)
            {
                float distance = start.TargetDistance;
                for(int i = start_seg_idx + 1; i < end_seg_idx; i++)
                    distance += seg[i].Length();
                distance += end.SourceDistance;
                var time = TimeUnitConversion * distance / edge.ForwardSpeed;

                if(options.IncludeConnectionDistance)
                {
                    var (cd, ct) = ConnectionDistanceTime(start, end, options.ConnectionSpeed);
                    distance += cd;
                    time += ct;
                }

                return new SingleEdgeResult{Found = true, Distance = distance, Time = time};
            }
            else if (!forward && edge.BackwardSpeed > 0)
            {
                float distance = end.SourceDistance;
                for(int i = end_seg_idx - 1; i > start_seg_idx; i--)
                    distance += seg[i].Length();
                distance += end.TargetDistance;
                var time = TimeUnitConversion * distance / edge.BackwardSpeed;

                if(options.IncludeConnectionDistance)
                {
                    var (cd, ct) = ConnectionDistanceTime(start, end, options.ConnectionSpeed);
                    distance += cd;
                    time += ct;
                }

                return new SingleEdgeResult{Found = true, Distance = distance, Time = time};
            }
        }

        return new SingleEdgeResult{Found = false};
    }

    internal static (float, float) ConnectionDistanceTime(ConnectionPoint start, ConnectionPoint end, float speed)
    {
        float sd = start.GetConnectionDistance();
        float ed = end.GetConnectionDistance();
        return (sd + ed, TimeUnitConversion * sd + ed / speed);
    }

    internal static bool EndPointToPointSearch(Connection end, Dictionary<uint, float> weights, float distance)
    {
        foreach(ConnectionPoint endP in end)
        {
            bool endCondidtion = false;
            if(endP.Segment != null)
            {
                var end_edge = endP.Segment.Edge;

                if (weights.TryGetValue(end_edge.Source.Id, out float sourceDistance) && weights.TryGetValue(end_edge.Target.Id, out float targetDistance))
                    if(distance > sourceDistance && distance > targetDistance)
                        endCondidtion = true;
            }
            if(!endCondidtion)
                return false;
        }
        return true;
    }

    internal static bool EndPointToPointTimeSearch(Connection end, Dictionary<uint, TimeDistanceResult> weights, float time)
    {
        foreach(ConnectionPoint endP in end)
        {
            bool endCondidtion = false;
            if(endP.Segment != null)
            {
                var end_edge = endP.Segment.Edge;

                if (weights.TryGetValue(end_edge.Source.Id, out var source) && weights.TryGetValue(end_edge.Target.Id, out var target))
                    if(time > source.Time && time > target.Time)
                        endCondidtion = true;
            }
            if(!endCondidtion)
                return false;
        }
        return true;
    }

    internal static bool EndPointToPointDistanceSearch(Connection end, Dictionary<uint, TimeDistanceResult> weights, float distance)
    {
        foreach(ConnectionPoint endP in end)
        {
            bool endCondidtion = false;
            if(endP.Segment != null)
            {
                var end_edge = endP.Segment.Edge;

                if (weights.TryGetValue(end_edge.Source.Id, out var source) && weights.TryGetValue(end_edge.Target.Id, out var target))
                    if(distance > source.Distance && distance > target.Distance)
                        endCondidtion = true;
            }
            if(!endCondidtion)
                return false;
        }
        return true;
    }

    internal static DistanceResult EndWeightsDistance(Connection end, Dictionary<uint, float> weights, QueryOptions options)
    {
        float min = float.MaxValue;
        foreach(ConnectionPoint endP in end)
        {
            if(endP.Segment != null)
            {
                var end_edge = endP.Segment.Edge;
                float connectionDistance = 0.0f;

                if(options.IncludeConnectionDistance)
                {
                    connectionDistance = endP.GetConnectionDistance();
                }

                
                if(end_edge.ForwardSpeed > 0.0f && weights.TryGetValue(end_edge.Source.Id, out var source))
                {
                    float tmp = source + endP.SourceDistance + connectionDistance;
                    if(tmp < min) min = tmp;
                }

                if(end_edge.BackwardSpeed > 0.0f && weights.TryGetValue(end_edge.Target.Id, out var target))
                {
                    float tmp = target + endP.TargetDistance + connectionDistance;
                    if(tmp < min) min = tmp;
                }
            }
        }
        if(min == float.MaxValue)
            return new DistanceResult{ HasResult = false };
        return new DistanceResult{Distance = min, HasResult = true };
    }

    internal static ITimeResult EndWeightsTime(Connection end, Dictionary<uint, float> weights, QueryOptions options)
    {
        float min = float.MaxValue;
        foreach(ConnectionPoint endP in end)
        {
            if(endP.Segment != null)
            {
                var end_edge = endP.Segment.Edge;
                float connectionTime = 0.0f;

                if(options.IncludeConnectionDistance)
                {
                    connectionTime = TimeUnitConversion * endP.GetConnectionDistance() / options.ConnectionSpeed;
                }

                if(end_edge.ForwardSpeed > 0.0f && weights.TryGetValue(end_edge.Source.Id, out var source))
                {
                    var tmp = source + (TimeUnitConversion * endP.SourceDistance / (float)end_edge.ForwardSpeed) + connectionTime;
                    if(tmp < min) min = tmp;
                }

                if(end_edge.BackwardSpeed > 0.0f && weights.TryGetValue(end_edge.Target.Id, out var target))
                {
                    var tmp = target + (TimeUnitConversion *  endP.TargetDistance / (float)end_edge.BackwardSpeed) + connectionTime;
                    if(tmp < min) min = tmp;   
                }
            }
        }
        if(min == float.MaxValue)
            return new TimeResult{ HasResult = false };
        return new TimeResult{ Time = min, HasResult = true };
    }

    internal static ITimeDistanceResult EndWeightsDistanceWithTime(Connection end, Dictionary<uint, TimeDistanceResult> weights, QueryOptions options)
    {
        float min = float.MaxValue;
        float time = 0;
        foreach(ConnectionPoint endP in end)
        {
            if(endP.Segment != null)
            {
                var end_edge = endP.Segment.Edge;
                float connectionDistance = 0.0f;
                float connectionTime = 0.0f;

                if(options.IncludeConnectionDistance)
                {
                    connectionDistance = endP.GetConnectionDistance();
                    connectionTime = TimeUnitConversion * endP.GetConnectionDistance() / options.ConnectionSpeed;
                }

                if(end_edge.ForwardSpeed > 0.0f && weights.TryGetValue(end_edge.Source.Id, out var source))
                {
                    float tmp = source.Distance + endP.SourceDistance + connectionDistance;
                    if(tmp < min)
                    {
                        min = tmp;
                        time = source.Time + (TimeUnitConversion * endP.SourceDistance / (float)end_edge.ForwardSpeed) + connectionTime;
                    }
                }

                if(end_edge.BackwardSpeed > 0.0f && weights.TryGetValue(end_edge.Target.Id, out var target))
                {
                    float tmp = target.Distance + endP.TargetDistance + connectionDistance;
                    if(tmp < min)
                    {
                        min = tmp;
                        time = target.Time + (TimeUnitConversion * endP.TargetDistance / (float)end_edge.BackwardSpeed) + connectionTime;
                    }
                }
            }
        }
        return new TimeDistanceResult{Distance = min, Time = time};
    }

    internal static ITimeDistanceResult EndWeightsTimeWithDistance(Connection end, Dictionary<uint, TimeDistanceResult> weights, QueryOptions options)
    {
        float min = float.MaxValue;
        float distance = 0;
        foreach(ConnectionPoint endP in end)
        {
            if(endP.Segment != null)
            {
                var end_edge = endP.Segment.Edge;
                float connectionDistance = 0.0f;
                float connectionTime = 0.0f;

                if(options.IncludeConnectionDistance)
                {
                    connectionDistance = endP.GetConnectionDistance();
                    connectionTime = TimeUnitConversion * endP.GetConnectionDistance() / options.ConnectionSpeed;
                }

                if(end_edge.ForwardSpeed > 0.0f && weights.TryGetValue(end_edge.Source.Id, out var source))
                {
                    var tmp = source.Time + (TimeUnitConversion * endP.SourceDistance / (float)end_edge.ForwardSpeed) + connectionTime;
                    if(tmp < min)
                    {
                        min = tmp;
                        distance = source.Distance + endP.SourceDistance + connectionDistance;
                    }
                }

                if(end_edge.BackwardSpeed > 0.0f && weights.TryGetValue(end_edge.Target.Id, out var target))
                {
                    var tmp = target.Time + (TimeUnitConversion *  endP.TargetDistance / (float)end_edge.BackwardSpeed) + connectionTime;
                    if(tmp < min)
                    {
                        min = tmp;
                        distance =  target.Distance + endP.TargetDistance + connectionDistance;
                    }
                }
            }
        }
        return new TimeDistanceResult{Distance = distance, Time = min};
    }

    internal static ConnectionPoint? StartWeightsNode(Connection start, Node node)
    {
        foreach(ConnectionPoint startP in start)
        {
            if(startP.Segment != null)
            {
                var startEdge = startP.Segment.Edge;
                if(startEdge.Source.Id == node.Id || startEdge.Target.Id == node.Id)
                {
                    return startP;
                }
            }
        }
        return null;
    }
    internal static (Node?, ConnectionPoint?) EndWeightsDistanceNode(Connection end, Dictionary<uint, float> weights, QueryOptions options)
    {
        float min = float.MaxValue;
        Node? node = null;
        ConnectionPoint? cp = null;
        foreach(ConnectionPoint endP in end)
        {
            if(endP.Segment != null)
            {
                var end_edge = endP.Segment.Edge;
                float connectionDistance = 0.0f;

                if(options.IncludeConnectionDistance)
                {
                    connectionDistance = endP.GetConnectionDistance();
                }

                if(end_edge.ForwardSpeed > 0.0f && weights.TryGetValue(end_edge.Source.Id, out var source))
                {
                    float tmp = source + endP.SourceDistance + connectionDistance;
                    if(tmp < min) 
                    {
                        min = tmp;
                        node = end_edge.Source;
                        cp = endP;
                    }
                }

                if(end_edge.BackwardSpeed > 0.0f && weights.TryGetValue(end_edge.Target.Id, out var target))
                {
                    float tmp = target + endP.TargetDistance + connectionDistance;
                    if(tmp < min) 
                    {
                        min = tmp;
                        node = end_edge.Target;
                        cp = endP;
                    }
                }
            }
        }
        return (node, cp);
    }

    internal static (Node?, ConnectionPoint?) EndWeightsTimeNode(Connection end, Dictionary<uint, float> weights, QueryOptions options)
    {
        float min = float.MaxValue;
        Node? node = null;
        ConnectionPoint? cp = null;
        foreach(ConnectionPoint endP in end)
        {
            if(endP.Segment != null)
            {
                var end_edge = endP.Segment.Edge;
                float connectionTime = 0.0f;

                if(options.IncludeConnectionDistance)
                {
                    connectionTime = TimeUnitConversion * endP.GetConnectionDistance() / options.ConnectionSpeed;
                }

                if(end_edge.ForwardSpeed > 0.0f && weights.TryGetValue(end_edge.Source.Id, out var source))
                {
                    var tmp = source + (TimeUnitConversion * endP.SourceDistance / (float)end_edge.ForwardSpeed) + connectionTime;
                    if(tmp < min) 
                    {
                        min = tmp;
                        node = end_edge.Source;
                        cp = endP;
                    }
                }

                if(end_edge.BackwardSpeed > 0.0f && weights.TryGetValue(end_edge.Target.Id, out var target))
                {
                    var tmp = target + (TimeUnitConversion *  endP.TargetDistance / (float)end_edge.BackwardSpeed) + connectionTime;
                    if(tmp < min) 
                    {
                        min = tmp;
                        node = end_edge.Target;
                        cp = endP;
                    }
                }
            }
        }
        return (node, cp);
    }
}