using NetTopologySuite.Geometries;
using Pipos.GeoLib.Core.Model;
using static Pipos.GeoLib.Road.Dijkstra.NetworkUtils;

namespace Pipos.GeoLib.Road.Dijkstra;

internal static class ShortestDistanceLineString
{
    internal static LineStringResult Query(Connection start, Connection end, Year year, QueryOptions options)
    {
        var result = FindSingleEdgeDistanceLineString(start, end, options);
        if(result != null)
        {
            return new LineStringResult{ HasResult = true, LineString = result};
        }

        List<Coordinate> line = new List<Coordinate>();
        PriorityQueue<Node, float> queue = new PriorityQueue<Node, float>();
        Dictionary<uint, float> weights = new Dictionary<uint, float>();
        Dictionary<uint, Edge> edges = new Dictionary<uint, Edge>();
        
        if(!StartWeightsDistance(start, weights, queue, options))
        {
            return LineStringResult.NoResult;
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

                float speed = current == edge.Source ? (float)edge.ForwardSpeed :  (float)edge.BackwardSpeed;
                if(speed > 0.0f)
                {
                    float new_distance = weights[current.Id] + edge.Distance;
                    if (weights.TryGetValue(next.Id, out float nextDistance)) 
                    {
                        if(nextDistance > new_distance)
                        {
                            weights[next.Id] = new_distance;
                            edges[next.Id] = edge;
                            queue.Enqueue(next, new_distance);
                        }
                    }
                    else
                    {
                        weights[next.Id] = new_distance;
                        edges[next.Id] = edge;
                        queue.Enqueue(next, new_distance);
                    }
                }
            }
        }

        var (nextNode, endPoint) = EndWeightsDistanceNode(end, weights, options);
        if(nextNode == null || endPoint == null)
            return LineStringResult.NoResult;

        if(options.IncludeConnectionDistance)
            line.Add(new Coordinate(endPoint.SearchX, endPoint.SearchY));
            
        endPoint.AddEndSegment(line, nextNode);

        while(edges.TryGetValue(nextNode.Id, out Edge? nextEdge))
        {
            nextEdge.AddSegment(line, nextNode);
            nextNode = nextEdge.GetOther(nextNode);
        }

        var startPoint = StartWeightsNode(start, nextNode);
        if(startPoint == null)
            return LineStringResult.NoResult;

        startPoint.AddStartSegment(line, nextNode);

        if(options.IncludeConnectionDistance)
            line.Add(new Coordinate(startPoint.SearchX, startPoint.SearchY));
    
        line.Reverse();
        return new LineStringResult{ HasResult = true, LineString = new LineString(line.ToArray())};
    }
}