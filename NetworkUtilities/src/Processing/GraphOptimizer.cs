using RoutingKit;
using Pipos.Common.NetworkUtilities.Processing;
using static Pipos.Common.NetworkUtilities.Model.PiposID;

public static class GraphOptimizer
{
    public static void PinActivityTile(List<Node> nodes, List<int> activityTiles)
    {
        KDIndex _index = new KDIndex(nodes.Count);
        for (var i = 0; i < nodes.Count; i++)
        {
            _index.Add(nodes[i].X, nodes[i].Y);
        }
        _index.Finish();

        for (var i = 0; i < activityTiles.Count; i++)
        {
            var x = XFromId(activityTiles[i]);
            var y = YFromId(activityTiles[i]);
            var radius = 250;
            var neighbours = _index.Within(x, y, radius);
            while (!neighbours.Any())
            {
                if (radius > 5000)
                {
                    break;
                }
                radius += 250;
                neighbours = _index.Within(x, y, radius);
            }

            if (neighbours.Any())
            {
                var node_idx = neighbours.OrderBy(idx => SquareDist(nodes[idx].X, nodes[idx].Y, x, y)).First();
                nodes[node_idx].Pinned = true;
            }
        }
    }
    public static void Optimize(List<Node> nodes)
    {
        var counter = nodes.Count;
        while (counter > 0)
        {
            counter = 0;
            foreach (var node in nodes)
            {
                if (node.NodeType == NodeType.Connection || node.Pinned)
                {
                    continue;
                }

                if (node.Edges.Count == 1)
                {
                    var edge = node.Edges[0];
                    var otherNode = edge.GetOtherNode(node);
                    edge.Source = null!;
                    edge.Target = null!;
                    otherNode.Edges.Remove(edge);
                    node.Edges.Clear();
                    counter++;
                    continue;
                }

                if (node.Edges.Count == 2)
                {
                    var leftEdge = node.Edges[0];
                    var rightEdge = node.Edges[1];

                    var leftNode = leftEdge.GetOtherNode(node);
                    var rightNode = rightEdge.GetOtherNode(node);

                    //  lf  rf
                    //  --> -->
                    // l---n---r
                    //   L   R
                    var lf = leftEdge.GetForwardTime(leftNode);
                    var rf = rightEdge.GetForwardTime(node);

                    //   lb  rb
                    //  <-- <--
                    // l---n---r
                    //   L   R
                    var rb = rightEdge.GetBackwardTime(rightNode);
                    var lb = leftEdge.GetBackwardTime(node);

                    // Någon av länkarna är enkelriktade och kan inte optimeras
                    if (lf == 0 || rf == 0 || rb == 0 || lb == 0)
                    {
                        continue;
                    }

                    leftEdge.Distance += rightEdge.Distance;

                    leftEdge.SetForwardTime(leftNode, lf + rf);
                    leftEdge.SetBackwardTime(leftNode, lb + rb);

                    if (leftNode == rightNode)
                    {
                        leftNode.Edges.Remove(leftEdge);
                        node.Edges.Remove(leftEdge);
                        leftEdge.Source = null!;
                        leftEdge.Target = null!;
                        counter++;
                        continue;
                    }

                    rightNode.Edges.Remove(rightEdge);
                    rightEdge.Source = null!;
                    rightEdge.Target = null!;

                    if (rightNode.Edges.Any(x => x.GetOtherNode(rightNode) == leftNode))
                    {
                         leftNode.Edges.Remove(leftEdge);
                         leftEdge.Source = null!;
                         leftEdge.Target = null!;
                    }
                    else
                    {
                        leftEdge.ReplaceNode(node, rightNode);
                        rightNode.Edges.Add(leftEdge);   
                    }

                    node.Edges.Clear();
                    counter++;
                    continue;
                }
            }
            Console.WriteLine($"Counter: {counter}");
        }
        var result = nodes.Where(n => n.Edges.Count > 0).ToArray();
        nodes.Clear();
        nodes.AddRange(result);
    }

    public static (int[] nodes, List<int> edges, List<int> weights) BuildAdjacencyLists(List<Node> graph)
    {
        int idx = 0;
        graph.ForEach(x => x.Index = idx++);

        var length = graph.Count;
        var nodes = new int[length];
        var edges = new List<int>();
        var weights = new List<int>();

        foreach (var node in graph)
        {
            nodes[node.Index] = edges.Count;
            foreach (var edge in node.Edges)
            {
                edges.Add((int)edge.GetOtherNode(node).Index);
                weights.Add((int)edge.Distance);
            }
        }

        return (nodes, edges, weights);
    }

    private static int SquareDist(int x1, int y1, int x2, int y2) 
    {
        var dx = x1 - x2;
        var dy = y1 - y2;
        return dx * dx + dy * dy;
    }

    // public static List<Edge> Optimize2(List<Node> graph)
    // {
    //     var (nodes, edges, weights) = BuildAdjacencyLists(graph);
    //     var sw = Stopwatch.StartNew();

    //     var counter = nodes.Length;
    //     var removedNodes = new bool[nodes.Length];
    //     var removedEdges = new bool[edges.Count];

    //     while (counter > 0)
    //     {
    //         sw.Restart();
    //         counter = 0;
    //         for (var i = 0; i < nodes.Length - 1; i++)
    //         {
    //             if (removedNodes[i])
    //             {
    //                 continue;
    //             }

    //             var nrOfEdges = nodes[i + 1] - nodes[i];

    //             if (nrOfEdges == 2)
    //             {
    //                 var rightNodeIdx = edges[nodes[i] + 1];
    //                 var leftNodeIdx = edges[nodes[i]];

    //                 //left
    //                 if (leftNodeIdx + 1 >= nodes.Length)
    //                 {
    //                     continue;
    //                 }

    //                 for (var e = nodes[leftNodeIdx]; e < nodes[leftNodeIdx + 1]; e++)
    //                 {
    //                     if (edges[e] == i)
    //                     {
    //                         edges[e] = rightNodeIdx;
    //                         weights[e] += weights[nodes[i] + 1];
    //                         break;
    //                     }
    //                 }

    //                 // right
    //                 if (rightNodeIdx + 1 >= nodes.Length)
    //                 {
    //                     continue;
    //                 }

    //                 for (var e = nodes[rightNodeIdx]; e < nodes[rightNodeIdx + 1]; e++)
    //                 {
    //                     if (edges[e] == i)
    //                     {
    //                         edges[e] = leftNodeIdx;
    //                         weights[e] += weights[nodes[i]];
    //                         break;
    //                     }
    //                 }

    //                 removedNodes[i] = true;
    //                 counter++;
    //             }
    //         }
    //         Console.WriteLine($"Counter {counter}, Elapsed: {sw.Elapsed}");
    //     }
    //     Console.WriteLine($"New size {removedNodes.Count(x => x == false)}");

    //     var result = new List<Edge>();
    //     for (var i = 0; i < nodes.Length - 1; i++)
    //     {
    //         if (removedNodes[i])
    //         {
    //             continue;
    //         }

    //         var o1 = graph[i];
    //         var node1 = new Node(o1.X, o1.Y, o1.NodeType);
    //         for (var e = nodes[i]; e < nodes[i + 1]; e++)
    //         {
    //             var o2 = graph[(int)edges[e]];
    //             var node2 = new Node(o2.X, o2.Y, o2.NodeType);
    //             var edge = new Edge(node1, node2, weights[e], weights[e]);
    //             result.Add(edge);
    //         }
    //     }

    //     return result;
    // }

    // public static List<Edge> Optimize2(List<Node> graph)
    // {
    //     var (nodes, edges, weights) = BuildAdjacencyLists(graph);
    //     var sw = Stopwatch.StartNew();

    //     var counter = nodes.Length;
    //     var removedNodes = new bool[nodes.Length];
    //     var removedEdges = new bool[edges.Count];

    //     while (counter > 0)
    //     {
    //         sw.Restart();
    //         counter = 0;
    //         for (var i = 0; i < nodes.Length - 1; i++)
    //         {
    //             if (removedNodes[i])
    //             {
    //                 continue;
    //             }

    //             var nrOfEdges = nodes[i + 1] - nodes[i];

    //             if (nrOfEdges == 1)
    //             {
    //                 var neighbourNode = edges[nodes[i]];
    //                 var nrOfNeighbourEdges = nodes[neighbourNode + 1] - nodes[neighbourNode];
    //                 if (nrOfNeighbourEdges == 2)
    //                 {
    //                     var edge = edges[nodes[neighbourNode + 1]] == i ? nodes[neighbourNode + 1] : nodes[neighbourNode];
    //                     removedNodes[i] = true;
    //                     removedEdges[nodes[i]] = true;
    //                     removedEdges[edge] = true;
    //                 }
    //                 continue;
    //             }

    //             if (nrOfEdges == 2)
    //             {
    //                 if (removedEdges[nodes[i] + 1] ^ removedEdges[nodes[i]])
    //                 {
    //                     var notRemovedEdge = removedEdges[nodes[i]] ? nodes[i] + 1 : nodes[i];
    //                     removedEdges[notRemovedEdge] = true;
    //                     removedNodes[i] = true;
    //                     continue;
    //                 }

    //                 var rightNodeIdx = edges[nodes[i] + 1];
    //                 var leftNodeIdx = edges[nodes[i]];

    //                 //left
    //                 if (leftNodeIdx + 1 >= nodes.Length)
    //                 {
    //                     continue;
    //                 }

    //                 for (var e = nodes[leftNodeIdx]; e < nodes[leftNodeIdx + 1]; e++)
    //                 {
    //                     if (edges[e] == i)
    //                     {
    //                         edges[e] = rightNodeIdx;
    //                         weights[e] += weights[nodes[i] + 1];
    //                         break;
    //                     }
    //                 }

    //                 // right
    //                 if (rightNodeIdx + 1 >= nodes.Length)
    //                 {
    //                     continue;
    //                 }

    //                 for (var e = nodes[rightNodeIdx]; e < nodes[rightNodeIdx + 1]; e++)
    //                 {
    //                     if (edges[e] == i)
    //                     {
    //                         edges[e] = leftNodeIdx;
    //                         weights[e] += weights[nodes[i]];
    //                         break;
    //                     }
    //                 }

    //                 removedNodes[i] = true;
    //                 counter++;
    //             }
    //         }
    //         Console.WriteLine($"Counter {counter}, Elapsed: {sw.Elapsed}");
    //     }
    //     Console.WriteLine($"New size {removedNodes.Count(x => x == false)}");

    //     var result = new List<Edge>();
    //     for (var i = 0; i < nodes.Length - 1; i++)
    //     {
    //         if (removedNodes[i])
    //         {
    //             continue;
    //         }

    //         var o1 = graph[i];
    //         var node1 = new Node(o1.X, o1.Y, o1.NodeType);
    //         for (var e = nodes[i]; e < nodes[i + 1]; e++)
    //         {
    //             var o2 = graph[(int)edges[e]];
    //             var node2 = new Node(o2.X, o2.Y, o2.NodeType);
    //             var edge = new Edge(node1, node2, weights[e], weights[e]);
    //             result.Add(edge);
    //         }
    //     }

    //     return result;
    // }

    // static void RemoveSingles(int[] nodes, List<int> edges, List<int> weights, bool[] removedNodes)
    // {
    //     for (var i = 0; i < nodes.Length - 1; i++)
    //     {
    //         if (removedNodes[i])
    //         {
    //             continue;
    //         }

    //         var nrOfEdges = nodes[i + 1] - nodes[i];
    //         if (nrOfEdges == 1)
    //         {
    //             var next = edges[nodes[i]];
    //             if (removedNodes[next])
    //             {
    //                 continue;
    //             }

    //             nrOfEdges = nodes[next + 1] - nodes[next];
    //             while (nrOfEdges == 2)
    //             {
    //                 removedNodes[next] = true;
    //                 next = edges[nodes[next]] == next ? edges[nodes[next] + 1] : edges[nodes[next]];
    //                 nrOfEdges = nodes[next + 1] - nodes[next];
    //             }
    //             continue;
    //         }
    //     }
    // }
}