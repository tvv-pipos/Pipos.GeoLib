using Pipos.GeoLib.Core.Api;
using Pipos.GeoLib.Core.Model;
using static Pipos.GeoLib.Road.Dijkstra.NetworkUtils;

namespace Pipos.GeoLib.Road.Dijkstra;

internal static class ShortestTime
{
    internal static ITimeResult Query(Connection start, Connection end, Year year, QueryOptions options)
    {        
        var result = FindSingleEdgeTime(start, end, options);
        if(result.HasResult)
        {
            return new TimeResult{ Time = result.Time, HasResult = true };
        }

        PriorityQueue<Node, float> queue = new PriorityQueue<Node, float>();
        Dictionary<uint, float> weights = new Dictionary<uint, float>();

        if(!StartWeightsTime(start, weights, queue, options))
        {
            return TimeResult.NoResult;
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

                float speed = current == edge.Source ? edge.ForwardSpeed :  edge.BackwardSpeed;
                if(speed > 0.0f)
                {
                    float newTime = weights[current.Id] + NetworkUtils.TimeUnitConversion * edge.Distance / speed;
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
        return EndWeightsTime(end, weights, options);
    }
}