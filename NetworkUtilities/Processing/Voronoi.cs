using System.Buffers;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Pipos.GeoLib.NetworkUtilities.Model;

namespace Pipos.GeoLib.NetworkUtilities.Processing;

public class Voronoi
{
    readonly ILogger<Voronoi> _logger;
    readonly ArrayPool<int> _arrayPool = ArrayPool<int>.Shared;

    public Voronoi(ILogger<Voronoi> logger)
    {
        _logger = logger;
    }

    public (int[] distances, int[] owners) CalculateAccessibility(List<Node> network, List<Node> startNodes, 
        SearchMode mode, int? maxSearchDistance)
    {
        var counter = 0;
        maxSearchDistance = maxSearchDistance ?? int.MaxValue;
        
        var sw = Stopwatch.StartNew();

        var distances = new int[network.Count];
        var owners = new int[network.Count];
        var queue = new PriorityQueue<Node, int>();

        foreach (var start in startNodes)
        {
            distances[start.Index] = 1;
            owners[start.Index] = start.Index;
            queue.Enqueue(start, 1);
        }

        while (queue.Count > 0)
        {
            var currentNode = queue.Dequeue();

            foreach (var edge in currentNode.Edges)
            {
                var target = edge.GetOtherNode(currentNode);
                var time = edge.GetForwardTime(currentNode);

                if (time == 0) // enkelriktad
                {
                    continue;
                }

                var weight = distances[currentNode.Index] + (
                    mode == SearchMode.Shortest ?
                        edge.Distance :
                        time
                );

                if ((distances[target.Index] == 0 || distances[target.Index] > weight) && weight < maxSearchDistance)
                {
                    // Om det inte är en startpunkt, så förhindra att punkter som är anslutna med 
                    // flera länkar till vägnätet används som bryggor/broar.
                    if (owners[currentNode.Index] != currentNode.Index 
                        && currentNode.NodeType == NodeType.Connection && edge.IsConnectionEdge)
                    {
                        continue;
                    }

                    counter++;
                    distances[target.Index] = weight;
                    owners[target.Index] = owners[currentNode.Index];
                    queue.Enqueue(target, weight);
                }
            }
        }
        _logger.LogInformation($"Counter: {counter}, Elapsed: {sw.Elapsed}");
        return (distances, owners);
    }

    public (Node? nearestNeightbour, int distance) 
    FindNearestNeighbour(List<Node> network, Node startNode, List<Node> targetNodes, 
        SearchMode mode, int? maxSearchDistance)
    {
        maxSearchDistance = maxSearchDistance ?? int.MaxValue;
        var counter = 0;
        var sw = Stopwatch.StartNew();
        var hashSet = targetNodes.ToHashSet();

        var distances = new int[network.Count];
        var queue = new MinHeap();

        distances[startNode.Index] = 1;
        queue.Add((uint)startNode.Index, 1);

        while (!queue.IsEmpty)
        {
            var index = queue.RemoveMin();
            var currentNode = network[(int)index];

            if (currentNode.NodeType == NodeType.Connection && hashSet.Contains(currentNode) && currentNode != startNode)
            {
                _logger.LogInformation($"Counter: {counter}, Elapsed: {sw.Elapsed}");
                return (currentNode, distances[currentNode.Index]);
            }

            // Passera inte genom anslutningsnoder, då blir dom som artificiella bryggor
            if (currentNode.NodeType == NodeType.Connection && currentNode != startNode)
            {
                continue;
            }

            foreach (var edge in currentNode.Edges)
            {
                var target = edge.GetOtherNode(currentNode);
                var time = edge.GetForwardTime(currentNode);

                if (time == 0) // enkelriktad
                {
                    continue;
                }

                var weight = distances[currentNode.Index] + (
                    mode == SearchMode.Shortest ?
                        edge.Distance :
                        time
                );

                if ((distances[target.Index] == 0 || distances[target.Index] > weight) && weight < maxSearchDistance)
                {
                    counter++;
                    distances[target.Index] = weight;
                    queue.Add((uint)target.Index, (uint)weight); 
                }
            }
        }

        _logger.LogInformation($"Counter: {counter}, Elapsed: {sw.Elapsed}");
        return (null, 0);
    }

    public (List<Node> nearestNeightbours, List<int> distances) 
    FindDistances(List<Node> network, Node startNode, List<Node> targetNodes, 
        SearchMode mode, int? maxSearchDistance)
    {
        maxSearchDistance = maxSearchDistance ?? int.MaxValue;
        var counter = 0;
        var hashSet = targetNodes.ToHashSet();

        var distances = _arrayPool.Rent(network.Count);
        var queue = new MinHeap();

        distances[startNode.Index] = 1;
        queue.Add((uint)startNode.Index, 1);

        var nearestNeightbours = new List<Node>();
        var shortestDistances = new List<int>();

        while (!queue.IsEmpty && nearestNeightbours.Count < targetNodes.Count)
        {
            var index = queue.RemoveMin();
            var currentNode = network[(int)index];

            if (hashSet.Contains(currentNode) && currentNode != startNode)
            {
                nearestNeightbours.Add(currentNode);
                shortestDistances.Add(distances[currentNode.Index]);
            }

            foreach (var edge in currentNode.Edges)
            {
                var target = edge.GetOtherNode(currentNode);
                var time = edge.GetForwardTime(currentNode);

                if (time == 0) // enkelriktad
                {
                    continue;
                }

                var weight = distances[currentNode.Index] + (
                    mode == SearchMode.Shortest ?
                        edge.Distance :
                        time
                );

                if ((distances[target.Index] == 0 || distances[target.Index] > weight) && weight < maxSearchDistance)
                {
                    //if (target.NodeType != NodeType.Default || !edge.IsConnectionEdge)
                    {
                        counter++;
                        distances[target.Index] = weight;
                        queue.Add((uint)target.Index, (uint)weight);
                    }
                }
            }
        }

        _arrayPool.Return(distances, true);
        return (nearestNeightbours, shortestDistances);
    }
}
