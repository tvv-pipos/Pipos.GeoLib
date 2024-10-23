using Pipos.GeoLib.Core.Api;
using Pipos.GeoLib.Core.Model;
using NetTopologySuite.Geometries;
using static Pipos.GeoLib.Road.Dijkstra.NetworkUtils;

namespace Pipos.GeoLib.Road.Dijkstra;

internal static class ShortestTimeLineString
{
    internal static ILineStringResult Query(Connection start, Connection end, Year year, QueryOptions options)
    {
        var result = FindSingleEdgeTimeLineString(start, end, options);
        if(result.HasResult)
        {
            return new LineStringResult{ 
                HasResult = true, 
                LineString = result.LineString,
                Time = result.Time,
                Distance = result.Distance};
        }

        List<Coordinate> line = new List<Coordinate>();
        PriorityQueue<Node, float> queue = new PriorityQueue<Node, float>();
        Dictionary<uint, TimeDistanceResult> weights = new Dictionary<uint, TimeDistanceResult>();
        Dictionary<uint, Edge> edges = new Dictionary<uint, Edge>();
        
        if(!StartWeightsTimeWithDistance(start, weights, queue, options))
        {
            return LineStringResult.NoResult;
        }

        while (queue.TryDequeue(out Node? current, out float distance)) 
        {
            if(EndPointToPointTimeSearch(end, weights, distance))
                break;
                
            for(int e = 0; e < current.Edges.Count; e++)
            {
                Edge edge = current.Edges[e];
                if(!edge.Years.HasYear(year))
                    continue;
             
                Node next = edge.GetOther(current);

                if(next == current)
                    continue;

                bool prohibited = edge.Attribute.ForwardProhibited; 
                float speed = edge.ForwardSpeed;

                if(current == edge.Target) 
                {
                    prohibited = edge.Attribute.BackwardProhibited; 
                    speed = edge.BackwardSpeed;
                } 

                if(!prohibited)
                {
                    float newDistance = weights[current.Id].Distance + edge.Distance;
                    float newTime = weights[current.Id].Time + NetworkUtils.TimeUnitConversion * edge.Distance / speed;

                    if (weights.TryGetValue(next.Id, out var weight)) 
                    {
                        if(weight.Time > newTime)
                        {
                            weights[next.Id] = new TimeDistanceResult{Distance = newDistance, Time = newTime};
                            edges[next.Id] = edge;
                            queue.Enqueue(next, newTime);
                        }
                    }
                    else
                    {
                        weights[next.Id] = new TimeDistanceResult{Distance = newDistance, Time = newTime};
                        edges[next.Id] = edge;
                        queue.Enqueue(next, newTime);
                    }
                }
            }
        }

        return EndWeightsTimeNode(start, end, weights, edges, options);
    }
}