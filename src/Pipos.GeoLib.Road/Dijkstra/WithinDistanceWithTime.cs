using Pipos.GeoLib.Core.Api;
using Pipos.GeoLib.Core.Model;
using Pipos.GeoLib.Road.Model;
using static Pipos.GeoLib.Road.Dijkstra.NetworkUtils;

namespace Pipos.GeoLib.Road.Dijkstra;

internal static class WithinDistanceWithTime
{
    internal static IWithinTimeDistanceResult Query(Connection start, float maxDistance, Year year, QueryOptions options)
    {
        Dictionary<uint, TimeDistanceResult> weights = new Dictionary<uint, TimeDistanceResult>();
        PriorityQueue<Node, float> queue = new PriorityQueue<Node, float>();
        
        if(!StartWeightsDistanceWithTime(start, weights, queue, options))
        {
            return WithinTimeDistanceResult.NoResult;
        }
        
        while (queue.TryDequeue(out Node? current, out float distance)) 
        {               
            for(int e = 0; e < current.Edges.Count; e++)
            {
                Edge edge = current.Edges[e];
                if(!edge.Years.HasYear(year))
                    continue;

                Node next = edge.GetOther(current);

                float speed = current == edge.Source ? edge.ForwardSpeed :  edge.BackwardSpeed;
                if(speed > 0.0f)
                {
                    float newDistance = weights[current.Id].Distance + edge.Distance;
                    float newTime = weights[current.Id].Time + TimeUnitConversion * edge.Distance / speed;

                    if(newDistance < maxDistance)
                    {
                        if (weights.TryGetValue(next.Id, out var weight)) 
                        {
                            if(weight.Distance > newDistance)
                            {
                                weights[next.Id] = new TimeDistanceResult{Distance = newDistance, Time = newTime, HasResult = true};
                                queue.Enqueue(next, newDistance);
                            }
                        }
                        else
                        {
                            weights[next.Id] = new TimeDistanceResult{Distance = newDistance, Time = newTime, HasResult = true};
                            queue.Enqueue(next, newDistance);
                        }
                    }
                }
            }
        }
        return new WithinTimeDistanceResult{ Weights = weights, HasResult = true };
    }
}