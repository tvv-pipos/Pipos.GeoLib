using RoutingKit;
using Pipos.Common.NetworkUtilities.Processing;
using static Pipos.Common.NetworkUtilities.Model.PiposID;
using System.Diagnostics;

public static class GraphOptimizer
{
    public static void PinActivityTile(List<Node> nodes, List<int> activityTiles)
    {
        Pipos250Index _index = new Pipos250Index();
        for (var i = 0; i < nodes.Count; i++)
        {
            _index.Add(nodes[i].X, nodes[i].Y);
        }
        //_index.Finish();

        for (var i = 0; i < activityTiles.Count; i++)
        {
            var node_idx = _index.FindNearest(activityTiles[i]);
            nodes[node_idx].Pinned = true;
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

                    /* TODO: Find other edge */
                    if (rightNode.Edges.Any(x => x.GetOtherNode(rightNode) == leftNode))
                    {
                         // Remove other edge
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

    public static void OptimizeNetwork(List<Node> nodes)
    {
        int c = 1;
        while (c > 0)
        {
            c = 0;
            foreach (var node in nodes)
            {
                if (node.NodeType == NodeType.Connection || node.Pinned)
                {
                    continue;
                }

                bool rl = false;
                if (node.Edges.Count() == 1)
                {
                    Edge l1 = node.Edges[0];
                    Node n1 = l1.GetOtherNode(node);
                    node.Edges.Clear(); 

                    rl = false;
                    for (int l = 0; l < n1.Edges.Count(); l++)
                    {
                        if (n1.Edges[l] == l1)
                        {
                            n1.Edges.Remove(n1.Edges[l]);
                            rl = true;
                            c++;
                            break;
                        }
                    }
                    Debug.Assert(rl);
                }
                else if (node.Edges.Count() == 2)
                {
                    Edge l1 = node.Edges[0];
                    Edge l2 = node.Edges[1];
                    Node n1 = l1.GetOtherNode(node);
                    Node n2 = l2.GetOtherNode(node);
                    if (n1 == n2)
                    {
                        c++;
                        node.Edges.Clear();
                        rl = false;
                        n1.Edges.Remove(l1);
                        n2.Edges.Remove(l2);
                    }
                    else if (l1.Source == node && l2.Target == node && l1.ForwardSpeed == l2.ForwardSpeed && l1.BackwardSpeed == l2.BackwardSpeed)
                    {
                        c++;
                        rl = false;
                        for (int l = 0; l < n1.Edges.Count(); l++)
                        {
                            if (n1.Edges[l] == l1)
                            {
                                n1.Edges[l] = l2;
                                rl = true;
                                break;
                            }
                        }
                        Debug.Assert(rl);
                        l1.Source = l2.Source;
                        l2.Target = l1.Target;
                        l2.Distance = l1.Distance + l2.Distance;
                        l2.ForwardTime = l1.ForwardTime + l2.ForwardTime;
                        l2.BackwardTime = l1.BackwardTime + l2.BackwardTime;
                        node.Edges.Clear();
                    }
                    else if (l1.Source == node && l2.Source == node && l1.ForwardSpeed == l2.BackwardSpeed && l1.BackwardSpeed == l2.ForwardSpeed)
                    {
                        c++;
                        rl = false;
                        for (int l = 0; l < n1.Edges.Count(); l++)
                        {
                            if (n1.Edges[l] == l1)
                            {
                                n1.Edges[l] = l2;
                                rl = true;
                                break;
                            }
                        }
                        Debug.Assert(rl);
                        l1.Source = l2.Target;
                        l2.Source = l1.Target;
                        l2.Distance = l1.Distance + l2.Distance;
                        l2.ForwardTime = l1.ForwardTime + l2.ForwardTime;
                        l2.BackwardTime = l1.BackwardTime + l2.BackwardTime;
                        node.Edges.Clear();
                    }
                    else if (l1.Target == node && l2.Target == node && l1.ForwardSpeed == l2.BackwardSpeed && l1.BackwardSpeed== l2.ForwardSpeed)
                    {
                        c++;
                        rl = false;
                        for (int l = 0; l < n1.Edges.Count(); l++)
                        {
                            if (n1.Edges[l] == l1)
                            {
                                n1.Edges[l] = l2;
                                rl = true;
                                break;
                            }
                        }
                        Debug.Assert(rl);
                        l1.Target = l2.Source;
                        l2.Target = l1.Source;
                        l2.Distance = l1.Distance + l2.Distance;
                        l2.ForwardTime = l1.ForwardTime + l2.ForwardTime;
                        l2.BackwardTime = l1.BackwardTime + l2.BackwardTime;
                        node.Edges.Clear();
                    }
                    else if (l1.Target == node && l2.Source == node && l1.ForwardSpeed == l2.ForwardSpeed && l1.BackwardSpeed == l2.BackwardSpeed)
                    {
                        c++;
                        rl = false;
                        for (int l = 0; l < n1.Edges.Count(); l++)
                        {
                            if (n1.Edges[l] == l1)
                            {
                                n1.Edges[l] = l2;
                                rl = true;
                                break;
                            }
                        }
                        Debug.Assert(rl);
                        l1.Target = l2.Target;
                        l2.Source = l1.Source;
                        l2.Distance = l1.Distance + l2.Distance;
                        l2.ForwardTime = l1.ForwardTime + l2.ForwardTime;
                        l2.BackwardTime = l1.BackwardTime + l2.BackwardTime;
                        node.Edges.Clear();
                    }
                }
            }
            Console.WriteLine($"Nodes eliminated = {c}\n");
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