using Pipos.GeoLib.Core.Api;
using Pipos.GeoLib.Core.Model;
using Pipos.GeoLib.Road.Model;
using static Pipos.GeoLib.Road.Dijkstra.NetworkUtils;

namespace Pipos.GeoLib.Road.Dijkstra;

internal static class WithinTimeWithDistance
{
    internal static IWithinTimeDistanceResult Query(Connection start, float maxTime, Year year, QueryOptions options)
    {
        Dictionary<uint, TimeDistanceResult> weights = new Dictionary<uint, TimeDistanceResult>();
        PriorityQueue<Node, float> queue = new PriorityQueue<Node, float>();
        
        if(!StartWeightsTimeWithDistance(start, weights, queue, options))
        {
            return WithinTimeDistanceResult.NoResult;
        }
        
        while (queue.TryDequeue(out Node? current, out float time)) 
        {               
            for(int e = 0; e < current.Edges.Count; e++)
            {
                Edge edge = current.Edges[e];
                if(!edge.Years.HasYear(year))
                    continue;

                Node next = edge.GetOther(current);

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
                    float newTime = weights[current.Id].Time + TimeUnitConversion * edge.Distance / speed;

                    if(newTime < maxTime)
                    {
                        if (weights.TryGetValue(next.Id, out var weight)) 
                        {
                            if(weight.Time > newTime)
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