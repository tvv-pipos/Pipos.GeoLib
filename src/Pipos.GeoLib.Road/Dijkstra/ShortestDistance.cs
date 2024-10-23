using Pipos.GeoLib.Core.Model;
using static Pipos.GeoLib.Road.Dijkstra.NetworkUtils;

namespace Pipos.GeoLib.Road.Dijkstra;

internal static class ShortestDistance
{
    internal static DistanceResult Query(Connection start, Connection end, Year year, QueryOptions options)
    {        
        var result = FindSingleEdgeDistance(start, end, options);
        if(result.HasResult)
        {
            return new DistanceResult{ Distance = result.Distance, HasResult = true };
        }

        PriorityQueue<Node, float> queue = new PriorityQueue<Node, float>();
        Dictionary<uint, float> weights = new Dictionary<uint, float>();
        
        if(!StartWeightsDistance(start, weights, queue, options))
        {
            return DistanceResult.NoResult;
        }

        while (queue.TryDequeue(out Node? current, out float distance)) 
        {
            if(EndPointToPointSearch(end, weights, distance))
                break;

            for(int e = 0; e < current.Edges.Count; e++)
            {
                Edge edge = current.Edges[e];
                if(!edge.Years.HasYear(year))
                    continue;

                Node next = edge.GetOther(current);

                if(next == current)
                    continue;

                bool prohibited = current == edge.Source ? edge.Attribute.ForwardProhibited : edge.Attribute.BackwardProhibited;
                if(!prohibited)
                {
                    float new_distance = weights[current.Id] + edge.Distance;
                    if (weights.TryGetValue(next.Id, out float nextDistance)) 
                    {
                        if(nextDistance > new_distance)
                        {
                            weights[next.Id] = new_distance;
                            queue.Enqueue(next, new_distance);
                        }
                    }
                    else
                    {
                        weights[next.Id] = new_distance;
                        queue.Enqueue(next, new_distance);
                    }
                }
            }
        }

        return EndWeightsDistance(end, weights, options);
    }
}