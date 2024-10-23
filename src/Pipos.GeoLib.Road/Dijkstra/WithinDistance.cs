using Pipos.GeoLib.Core.Api;
using Pipos.GeoLib.Core.Model;
using Pipos.GeoLib.Road.Model;
using static Pipos.GeoLib.Road.Dijkstra.NetworkUtils;

namespace Pipos.GeoLib.Road.Dijkstra;

internal static class WithinDistance
{
    internal static IWithinDistanceResult Query(Connection start, float maxDistance, Year year, QueryOptions options)
    {
        Dictionary<uint, float> weights = new Dictionary<uint, float>();
        PriorityQueue<Node, float> queue = new PriorityQueue<Node, float>();
        
        if(!StartWeightsDistance(start, weights, queue, options))
        {
            return WithinDistanceResult.NoResult;
        }

        while (queue.TryDequeue(out Node? current, out float distance)) 
        {               
            for(int e = 0; e < current.Edges.Count; e++)
            {
                Edge edge = current.Edges[e];
                if(!edge.Years.HasYear(year))
                    continue;

                Node next = edge.GetOther(current);

                bool prohibited = current == edge.Source ? edge.Attribute.ForwardProhibited : edge.Attribute.BackwardProhibited;
                if(!prohibited)
                {
                    float newDistance = weights[current.Id] + edge.Distance;
                    if(newDistance < maxDistance)
                    {
                        if (weights.TryGetValue(next.Id, out float nextDistance)) 
                        {
                            if(nextDistance > newDistance)
                            {
                                weights[next.Id] = newDistance;
                                queue.Enqueue(next, newDistance);
                            }
                        }
                        else
                        {
                            weights[next.Id] = newDistance;
                            queue.Enqueue(next, newDistance);
                        }
                    }
                }
            }
        }
        return new WithinDistanceResult{ Weights = weights, HasResult = true };
    }
}