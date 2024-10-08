using Pipos.GeoLib.Core.Api;
using Pipos.GeoLib.Core.Model;
using Pipos.GeoLib.Road.Model;
using static Pipos.GeoLib.Road.Dijkstra.NetworkUtils;

namespace Pipos.GeoLib.Road.Dijkstra;

internal static class WithinTime
{
    internal static IWithinTimeResult Query(Connection start, float maxTime, Year year, QueryOptions options)
    {   
        Dictionary<uint, float> weights = new Dictionary<uint, float>();
        PriorityQueue<Node, float> queue = new PriorityQueue<Node, float>();
        
        if(!StartWeightsTime(start, weights, queue, options))
        {
            return WithinTimeResult.NoResult;
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
                    float newTime = weights[current.Id] + TimeUnitConversion * edge.Distance / speed;
                    if(newTime < maxTime)
                    {
                        if (weights.TryGetValue(next.Id, out float nextTime)) 
                        {
                            if(nextTime > newTime)
                            {
                                weights[next.Id] = newTime;
                                queue.Enqueue(next, newTime);
                            }
                        }
                        else
                        {
                            weights[next.Id] = newTime;
                            queue.Enqueue(next, newTime);
                        }
                    }
                }
            }
        }
        return new WithinTimeResult{ Weights = weights, HasResult = true };
    }
}