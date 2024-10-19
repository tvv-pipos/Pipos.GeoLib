using Pipos.GeoLib.Core.Api;
using Pipos.GeoLib.Core.Model;
using static Pipos.GeoLib.Road.Dijkstra.NetworkUtils;

namespace Pipos.GeoLib.Road.Dijkstra;

internal static class ShortestDistanceWithTime
{
    internal static ITimeDistanceResult Query(Connection start, Connection end, Year year, QueryOptions options)
    {        
        var result = FindSingleEdgeDistance(start, end, options);
        if(result.HasResult)
        {
            return new TimeDistanceResult{Distance = result.Distance, Time = result.Time};
        }

        PriorityQueue<Node, float> queue = new PriorityQueue<Node, float>();
        Dictionary<uint, TimeDistanceResult> weights = new Dictionary<uint, TimeDistanceResult>();
        
        if(!StartWeightsDistanceWithTime(start, weights, queue, options))
        {
            return TimeDistanceResult.NoResult;
        }

        while (queue.TryDequeue(out Node? current, out float distance)) 
        {
            if(EndPointToPointDistanceSearch(end, weights, distance))
                break;

            for(int e = 0; e < current.Edges.Count; e++)
            {
                Edge edge = current.Edges[e];
                if(!edge.Years.HasYear(year))
                    continue;

                Node next = edge.GetOther(current);

                if(next == current)
                    continue;

                float speed = current == edge.Source ? (float)edge.ForwardSpeed :  (float)edge.BackwardSpeed;
                if(speed > 0.0f)
                {
                    float newDistance = weights[current.Id].Distance + edge.Distance;
                    float newTime = weights[current.Id].Time + TimeUnitConversion * edge.Distance / speed;

                    if (weights.TryGetValue(next.Id, out var weight)) 
                    {
                        if(weight.Distance > newDistance)
                        {
                            weights[next.Id] = new TimeDistanceResult{Distance = newDistance, Time = newTime};
                            queue.Enqueue(next, newDistance);
                        }
                    }
                    else
                    {
                        weights[next.Id] = new TimeDistanceResult{Distance = newDistance, Time = newTime};
                        queue.Enqueue(next, newDistance);
                    }
                }
            }
        }

        return EndWeightsDistanceWithTime(end, weights, options);
    }
}