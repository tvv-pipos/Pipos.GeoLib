using System.Diagnostics;

namespace RoutingKit;

public class ShorterPathTest
{
    private readonly int _maxPopCount;
    private readonly Graph _graph;
    private readonly int[] _forwardTentativeDistance;
    private readonly int[] _backwardTentativeDistance;
    private readonly MinIDQueue _forwardQueue;
    private readonly MinIDQueue _backwardQueue;
    private readonly TimestampFlags _wasForwardPushed;
    private readonly TimestampFlags _wasBackwardPushed;
    private int _bypassNode;

    public ShorterPathTest(Graph graph, int maxPopCount)
    {
        _maxPopCount = maxPopCount;
        _graph = graph;
        _forwardTentativeDistance = new int[graph.NodeCount];
        _backwardTentativeDistance = new int[graph.NodeCount];
        _forwardQueue = new MinIDQueue(graph.NodeCount);
        _backwardQueue = new MinIDQueue(graph.NodeCount);
        _wasForwardPushed = new TimestampFlags(graph.NodeCount);
        _wasBackwardPushed = new TimestampFlags(graph.NodeCount);
    }

    public void PinSource(int s, int new_bypass_node)
    {
        _wasForwardPushed.ResetAll();
        _forwardQueue.Clear();
        _forwardQueue.Push(s, 0);
        _forwardTentativeDistance[s] = 0;
        _wasForwardPushed.Set(s);
        _bypassNode = new_bypass_node;
    }

    static Arc getOutArc(Graph graph, int node, int arc) => graph.Out(node, arc);
    static Arc getInArc(Graph graph, int node, int arc) => graph.In(node, arc);
    static int getOutDeg(Graph graph, int node) => graph.OutDeg(node);
    static int getInDeg(Graph graph, int node) => graph.InDeg(node);

    public static long counter = 0;

    static bool ForwardSettle(
        MinIDQueue forwardQueue,
        TimestampFlags wasForwardPushed,
        TimestampFlags wasBackwardPushed,
        int[] forwardTentativeDistance,
        int[] backwardTentativeDistance,
        bool isOut,
        Graph graph,
        int bypass,
        int len)
    {
        counter++;
        var p = forwardQueue.Pop();

        int poppedNode = p.Id;
        int distanceToPoppedNode = p.Key;

        Debug.Assert(forwardTentativeDistance[poppedNode] == distanceToPoppedNode);
        Debug.Assert(wasForwardPushed.IsSet(poppedNode));

        if (wasBackwardPushed.IsSet(poppedNode))
        {
            if (distanceToPoppedNode + backwardTentativeDistance[poppedNode] <= len)
            {
                return true;
            }
        }

        bool witnessFound = false;
        var graphOutDegResult = isOut ? getOutDeg(graph, poppedNode) : getInDeg(graph, poppedNode);
        for (int outArc = 0; outArc < graphOutDegResult; ++outArc)
        {
            var nextArc = isOut ? getOutArc(graph, poppedNode, outArc) : getInArc(graph, poppedNode, outArc);
            int nextNode = nextArc.Node;

            if (nextNode == bypass)
                continue;

            int nextNodeDistance = distanceToPoppedNode + nextArc.Weight;

            if (wasForwardPushed.IsSet(nextNode))
            {
                if (nextNodeDistance < forwardTentativeDistance[nextNode])
                {
                    forwardQueue.DecreaseKey(nextNode, nextNodeDistance);
                    forwardTentativeDistance[nextNode] = nextNodeDistance;

                    if (wasBackwardPushed.IsSet(nextNode))
                    {
                        if (nextNodeDistance + backwardTentativeDistance[nextNode] <= len)
                        {
                            witnessFound = true;
                        }
                    }
                }
            }
            else
            {
                wasForwardPushed.Set(nextNode);
                forwardTentativeDistance[nextNode] = nextNodeDistance;
                forwardQueue.Push(nextNode, nextNodeDistance);

                if (wasBackwardPushed.IsSet(nextNode))
                {
                    if (nextNodeDistance + backwardTentativeDistance[nextNode] <= len)
                    {
                        witnessFound = true;
                    }
                }
            }
        }

        return witnessFound;
    }

    public bool DoesShorterOrEqualPathToTargetExist(int t, int len)
    {
        _wasBackwardPushed.ResetAll();
        _backwardQueue.Clear();
        _backwardQueue.Push(t, 0);
        _backwardTentativeDistance[t] = 0;
        _wasBackwardPushed.Set(t);

        int popCount = 0;

        if (_wasForwardPushed.IsSet(t))
        {
            if (_forwardTentativeDistance[t] <= len)
            {
                return true;
            }
        }        

        while (!_forwardQueue.IsEmpty && !_backwardQueue.IsEmpty)
        {
            if (_forwardQueue.Peek().Key + _backwardQueue.Peek().Key > len)
            {
                return false;
            }

            if (_forwardQueue.Peek().Key <= _backwardQueue.Peek().Key)
            {
                if (ForwardSettle(
                    _forwardQueue,
                    _wasForwardPushed,
                    _wasBackwardPushed,
                    _forwardTentativeDistance,
                    _backwardTentativeDistance,
                    true,
                    _graph,
                    _bypassNode,
                    len))
                {
                    return true;
                }
            }
            else
            {
                if (ForwardSettle(
                    _backwardQueue,
                    _wasBackwardPushed,
                    _wasForwardPushed,
                    _backwardTentativeDistance,
                    _forwardTentativeDistance,
                    false,
                    _graph,
                    _bypassNode,
                    len))
                {
                    return true;
                }
            }

            ++popCount;

            if (popCount > _maxPopCount)
            {
                return false;
            }
        }

        return false;
    }

}



