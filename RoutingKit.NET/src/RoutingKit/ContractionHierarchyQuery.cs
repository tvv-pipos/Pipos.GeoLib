using System.Diagnostics;

namespace RoutingKit;

public class ContractionHierarchyQuery
{
    private ContractionHierarchy ch;

    private TimestampFlags wasForwardPushed, wasBackwardPushed;
    private MinIDQueue forwardQueue, backwardQueue;
    private int[] forwardTentativeDistance, backwardTentativeDistance;
    private int[] forwardPredecessorNode, backwardPredecessorNode;
    private int[] forwardPredecessorArc, backwardPredecessorArc;
    private int shortestPathMeetingNode;
    private int manyToManySourceOrTargetCount;

    private enum InternalState : int
    {
        Initialized,
        Run,
        SourcePinned,
        SourceRun,
        TargetPinned,
        TargetRun
    }

    private InternalState state;


    public ContractionHierarchyQuery(ContractionHierarchy ch)
    {
        this.ch = ch;
        this.wasForwardPushed = new TimestampFlags(ch.NodeCount);
        wasBackwardPushed = new TimestampFlags(ch.NodeCount);
        forwardQueue = new MinIDQueue(ch.NodeCount);
        backwardQueue = new MinIDQueue(ch.NodeCount);
        forwardTentativeDistance = new int[ch.NodeCount];
        backwardTentativeDistance = new int[ch.NodeCount];
        forwardPredecessorNode = new int[ch.NodeCount];
        backwardPredecessorNode = new int[ch.NodeCount];
        forwardPredecessorArc = new int[ch.NodeCount];
        backwardPredecessorArc = new int[ch.NodeCount];
        shortestPathMeetingNode = Constants.INVALID_ID;
        state = InternalState.Initialized;
    }

    public ContractionHierarchyQuery Reset()
    {
        if (ch == null)
        {
            throw new InvalidOperationException("Query object must have an attached CH.");
        }

        wasForwardPushed.ResetAll();
        forwardQueue.Clear();
        wasBackwardPushed.ResetAll();
        backwardQueue.Clear();

        shortestPathMeetingNode = Constants.INVALID_ID;

        state = InternalState.Initialized;
        return this;
    }

    public ContractionHierarchyQuery Reset(ContractionHierarchy newCh)
    {
        if (forwardTentativeDistance.Length == newCh.NodeCount)
        {
            Reset();
            ch = newCh;
            return this;
        }

        return new ContractionHierarchyQuery(newCh);
    }

    public ContractionHierarchyQuery ResetSource()
    {
        Debug.Assert(ch != null, "Query object must have an attached CH");
        Debug.Assert(state == InternalState.TargetPinned || state == InternalState.TargetRun);

        wasForwardPushed.ResetAll();
        forwardQueue.Clear();

        state = InternalState.TargetPinned;
        return this;
    }

    public ContractionHierarchyQuery ResetTarget()
    {
        Debug.Assert(ch != null, "Query object must have an attached CH");
        Debug.Assert(state == InternalState.SourcePinned || state == InternalState.SourceRun);

        wasBackwardPushed.ResetAll();
        backwardQueue.Clear();

        state = InternalState.SourcePinned;
        return this;
    }


    public ContractionHierarchyQuery AddSource(int externalS, int distToS = 0)
    {
        Debug.Assert(ch != null, "Query object must have an attached CH");
        Debug.Assert(externalS < ch.NodeCount, "Node out of bounds");
        Debug.Assert(state == InternalState.Initialized || state == InternalState.TargetPinned);

        int s = ch.Rank[externalS];

        if (!forwardQueue.ContainsId(s))
        {
            forwardQueue.Push(s, distToS);
            forwardTentativeDistance[s] = distToS;
            forwardPredecessorNode[s] = Constants.INVALID_ID;
        }
        else
        {
            if (distToS < forwardTentativeDistance[s])
            {
                forwardTentativeDistance[s] = distToS;
                forwardQueue.DecreaseKey(s, distToS);
            }
        }

        wasForwardPushed.Set(s);
        return this;
    }

    public ContractionHierarchyQuery AddTarget(int externalT, int distToT = 0)
    {
        Debug.Assert(ch != null, "Query object must have an attached CH");
        Debug.Assert(externalT < ch.NodeCount, "Node out of bounds");
        Debug.Assert(state == InternalState.Initialized || state == InternalState.SourcePinned);

        int t = ch.Rank[externalT];
        if (!backwardQueue.ContainsId(t))
        {
            backwardQueue.Push(t, distToT);
            backwardTentativeDistance[t] = distToT;
            backwardPredecessorNode[t] = Constants.INVALID_ID;
        }
        else
        {
            if (distToT < backwardTentativeDistance[t])
            {
                backwardTentativeDistance[t] = distToT;
                backwardQueue.DecreaseKey(t, distToT);
            }
        }

        wasBackwardPushed.Set(t);
        return this;
    }

    public ContractionHierarchyQuery Run()
    {
        Debug.Assert(ch != null, "Query object must have an attached CH");
        Debug.Assert(!forwardQueue.IsEmpty, "Must add at least one source before calling run");
        Debug.Assert(!backwardQueue.IsEmpty, "Must add at least one target before calling run");
        Debug.Assert(state == InternalState.Initialized);

        int shortestPathLength = Constants.INF_WEIGHT;
        shortestPathMeetingNode = Constants.INVALID_ID;

        bool forwardNext = true;

        while (true)
        {
            bool forwardFinished = false;
            if (forwardQueue.IsEmpty)
                forwardFinished = true;
            else if (forwardQueue.Peek().Key >= shortestPathLength)
                forwardFinished = true;

            bool backwardFinished = false;
            if (backwardQueue.IsEmpty)
                backwardFinished = true;
            else if (backwardQueue.Peek().Key >= shortestPathLength)
                backwardFinished = true;

            if (forwardFinished && backwardFinished)
                break;

            if (forwardFinished)
                forwardNext = false;
            if (backwardFinished)
                forwardNext = true;

            if (forwardNext)
            {
                ForwardSettleNode(ref shortestPathLength, ref shortestPathMeetingNode,
                    ch.Forward.FirstOut, ch.Forward.Head, ch.Forward.Weight,
                    ch.Backward.FirstOut, ch.Backward.Head, ch.Backward.Weight,
                    wasForwardPushed, wasBackwardPushed,
                    forwardQueue,
                    forwardTentativeDistance, backwardTentativeDistance,
                    forwardPredecessorNode, forwardPredecessorArc);
                forwardNext = false;
            }
            else
            {
                ForwardSettleNode(ref shortestPathLength, ref shortestPathMeetingNode,
                    ch.Backward.FirstOut, ch.Backward.Head, ch.Backward.Weight,
                    ch.Forward.FirstOut, ch.Forward.Head, ch.Forward.Weight,
                    wasBackwardPushed, wasForwardPushed,
                    backwardQueue,
                    backwardTentativeDistance, forwardTentativeDistance,
                    backwardPredecessorNode, backwardPredecessorArc);
                forwardNext = true;
            }
        }

        state = InternalState.Run;
        return this;
    }

    public ContractionHierarchyQuery RunToPinnedTargets()
    {
        Debug.Assert(ch != null, "Query object must have an attached CH");
        Debug.Assert(!forwardQueue.IsEmpty, "Must add at least one source before calling run");
        Debug.Assert(state == InternalState.TargetPinned);

        PinnedRun(
            backwardTentativeDistance,
            shortestPathMeetingNode,

            wasForwardPushed,
            forwardQueue,
            forwardTentativeDistance,

            forwardPredecessorNode,
            forwardPredecessorArc,

            ch.Forward.FirstOut,
            ch.Forward.Head,
            ch.Forward.Weight,

            ch.Backward.FirstOut,
            ch.Backward.Head,
            ch.Backward.Weight
        );

        state = InternalState.TargetRun;
        return this;
    }

    public ContractionHierarchyQuery RunToPinnedSources()
    {
        Debug.Assert(ch != null, "Query object must have an attached CH");
        Debug.Assert(!backwardQueue.IsEmpty, "Must add at least one target before calling run");
        Debug.Assert(state == InternalState.SourcePinned, "Invalid state for run_to_pinned_sources");

        PinnedRun(
            forwardTentativeDistance,
            shortestPathMeetingNode,
            wasBackwardPushed,
            backwardQueue,
            backwardTentativeDistance,
            backwardPredecessorNode,
            backwardPredecessorArc,
            ch.Backward.FirstOut,
            ch.Backward.Head,
            ch.Backward.Weight,
            ch.Forward.FirstOut,
            ch.Forward.Head,
            ch.Forward.Weight);

        state = InternalState.SourceRun;
        return this;
    }


    private static void PinnedRun(
        int[] selectList,
        int selectCount,

        TimestampFlags hasForwardPredecessor,
        MinIDQueue forwardQueue,
        int[] tentativeDistance,

        int[] forwardPredecessorNode,
        int[] predecessorArc,

        int[] forwardFirstOut,
        List<int> forwardHead,
        List<int> forwardWeight,

        int[] backwardFirstOut,
        List<int> backwardHead,
        List<int> backwardWeight)
    {
        FullForwardSearch(
            forwardFirstOut, forwardHead, forwardWeight,
            hasForwardPredecessor,
            forwardQueue,
            tentativeDistance,
            forwardPredecessorNode,
            predecessorArc
        );

        for (int i = 0; i < selectCount; ++i)
        {
            int x = selectList[i];
            int dist = Constants.INF_WEIGHT;
            int pred = Constants.INVALID_ID;
            if (hasForwardPredecessor.IsSet(x))
                dist = tentativeDistance[x];

            for (int xy = backwardFirstOut[x]; xy < backwardFirstOut[x + 1]; ++xy)
            {
                int y = backwardHead[xy];
                int newDist = tentativeDistance[y] + backwardWeight[xy];
                if (newDist < dist)
                {
                    dist = newDist;
                    pred = xy;
                }
            }

            if (pred != Constants.INVALID_ID)
            {
                tentativeDistance[x] = dist;
                predecessorArc[x] = pred;
                hasForwardPredecessor.ResetOne(x);
            }
            else if (dist == Constants.INF_WEIGHT)
            {
                tentativeDistance[x] = Constants.INF_WEIGHT;
                predecessorArc[x] = Constants.INVALID_ID;
            }
        }
    }

    private static void FullForwardSearch(
        int[] forwardFirstOut, List<int> forwardHead, List<int> forwardWeight,
        TimestampFlags wasForwardPushed,
        MinIDQueue forwardQueue,
        int[] forwardTentativeDistance,
        int[] forwardPredecessorNode,
        int[] forwardPredecessorArc)
    {
        while (!forwardQueue.IsEmpty)
        {
            var p = forwardQueue.Pop();
            var poppedNode = p.Id;
            var distanceToPoppedNode = p.Key;

            ForwardExpandUpwardChArcsOfNode(
                poppedNode, distanceToPoppedNode,
                forwardFirstOut, forwardHead, forwardWeight,
                wasForwardPushed, forwardQueue,
                forwardTentativeDistance,
                (x, predNode, predArc) =>
                {
                    forwardPredecessorNode[x] = predNode;
                    forwardPredecessorArc[x] = predArc;
                });
        }
    }


    private void ForwardSettleNode(ref int shortestPathLength, ref int shortestPathMeetingNode,
        int[] forwardFirstOut, List<int> forwardHead, List<int> forwardWeight,
        int[] backwardFirstOut, List<int> backwardHead, List<int> backwardWeight,
        TimestampFlags wasForwardPushed, TimestampFlags wasBackwardPushed,
        MinIDQueue forwardQueue,
        int[] forwardTentativeDistance, int[] backwardTentativeDistance,
        int[] forwardPredecessorNode, int[] forwardPredecessorArc)
    {
        var p = forwardQueue.Pop();
        var poppedNode = p.Id;
        var distanceToPoppedNode = p.Key;

        if (wasBackwardPushed.IsSet(poppedNode))
        {
            if (shortestPathLength > distanceToPoppedNode + backwardTentativeDistance[poppedNode])
            {
                shortestPathLength = distanceToPoppedNode + backwardTentativeDistance[poppedNode];
                shortestPathMeetingNode = poppedNode;
            }
        }

        if (!ForwardCanStallAtNode(poppedNode, backwardFirstOut, backwardHead, backwardWeight,
            wasForwardPushed, forwardTentativeDistance, backwardTentativeDistance))
        {
            ForwardExpandUpwardChArcsOfNode(poppedNode, distanceToPoppedNode, forwardFirstOut, forwardHead, forwardWeight,
                wasForwardPushed, forwardQueue, forwardTentativeDistance,
                (x, predNode, predArc) =>
                {
                    forwardPredecessorNode[x] = predNode;
                    forwardPredecessorArc[x] = predArc;
                });
        }
    }

    private static void ForwardExpandUpwardChArcsOfNode(int node, int distanceToNode,
        int[] forwardFirstOut, List<int> forwardHead, List<int> forwardWeight,
        TimestampFlags wasForwardPushed, MinIDQueue forwardQueue,
        int[] forwardTentativeDistance, Action<int, int, int> setPredecessor)
    {
        for (int arc = forwardFirstOut[node]; arc < forwardFirstOut[node + 1]; ++arc)
        {
            int h = forwardHead[arc];
            int d = distanceToNode + forwardWeight[arc];

            if (wasForwardPushed.IsSet(h))
            {
                if (d < forwardTentativeDistance[h])
                {
                    forwardQueue.DecreaseKey(h, d);
                    forwardTentativeDistance[h] = d;
                    setPredecessor(h, node, arc);
                }
            }
            else if (d < Constants.INF_WEIGHT)
            {
                forwardQueue.Push(h, d);
                forwardTentativeDistance[h] = d;
                wasForwardPushed.Set(h);
                setPredecessor(h, node, arc);
            }
        }
    }

    private bool ForwardCanStallAtNode(int node, int[] backwardFirstOut, List<int> backwardHead, List<int> backwardWeight,
        TimestampFlags wasForwardPushed, int[] forwardTentativeDistance, int[] backwardTentativeDistance)
    {
        for (int arc = backwardFirstOut[node]; arc < backwardFirstOut[node + 1]; ++arc)
        {
            int x = backwardHead[arc];

            if (wasForwardPushed.IsSet(x))
            {
                if (forwardTentativeDistance[x] + backwardWeight[arc] <= forwardTentativeDistance[node])
                {
                    return true;
                }
            }
        }

        return false;
    }

    public int GetDistance()
    {
        Debug.Assert(state == InternalState.Run);

        if (shortestPathMeetingNode == Constants.INVALID_ID)
        {
            return Constants.INF_WEIGHT;
        }
        return forwardTentativeDistance[shortestPathMeetingNode] + backwardTentativeDistance[shortestPathMeetingNode];
    }

    public int[] GetDistancesToSources()
    {
        Debug.Assert(state == InternalState.SourceRun);
        return ExtractDistancesToTargets(forwardPredecessorNode, manyToManySourceOrTargetCount, backwardTentativeDistance);
    }

    public void GetDistancesToSources(int[] dist)
    {
        Debug.Assert(state == InternalState.SourceRun);
        ExtractDistancesToTargets(forwardPredecessorNode, manyToManySourceOrTargetCount, backwardTentativeDistance, dist);
    }

    public int[] GetDistancesToTargets()
    {
        Debug.Assert(state == InternalState.TargetRun);
        return ExtractDistancesToTargets(backwardPredecessorNode, manyToManySourceOrTargetCount, forwardTentativeDistance);
    }

    public void GetDistanceToTargets(int[] dist)
    {
        Debug.Assert(state == InternalState.TargetRun);
        ExtractDistancesToTargets(backwardPredecessorNode, manyToManySourceOrTargetCount, forwardTentativeDistance, dist);
    }

    public int[] ExtractDistancesToTargets(
        int[] targetList,
        int targetCount,
        int[] forwardTentativeDistance)
    {
        var dist = new int[targetCount];
        ExtractDistancesToTargets(targetList, targetCount, forwardTentativeDistance, dist);
        return dist;
    }

    public void ExtractDistancesToTargets(
        int[] targetList,
        int targetCount,
        int[] forwardTentativeDistance,
        int[] dist)
    {
        for (int i = 0; i < targetCount; ++i)
        {
            dist[i] = forwardTentativeDistance[targetList[i]];
        }
    }

    public void UnpackForwardArc(ContractionHierarchy ch, int arc, Action<int, int> onNewInputArc)
    {
        if (ch.Forward.IsShortcutAnOriginalArc[arc])
        {
            onNewInputArc(ch.Forward.ShortcutFirstArc[arc], ch.Forward.ShortcutSecondArc[arc]);
        }
        else
        {
            Debug.Assert(ch.Forward.ShortcutFirstArc[arc] < ch.Backward.Head.Count);
            Debug.Assert(ch.Forward.ShortcutSecondArc[arc] < ch.Forward.Head.Count);
            UnpackBackwardArc(ch, ch.Forward.ShortcutFirstArc[arc], onNewInputArc);
            UnpackForwardArc(ch, ch.Forward.ShortcutSecondArc[arc], onNewInputArc);
        }
    }

    public void UnpackBackwardArc(ContractionHierarchy ch, int arc, Action<int, int> onNewInputArc)
    {
        if (ch.Backward.IsShortcutAnOriginalArc[arc])
        {
            onNewInputArc(ch.Backward.ShortcutFirstArc[arc], ch.Backward.ShortcutSecondArc[arc]);
        }
        else
        {
            Debug.Assert(ch.Backward.ShortcutFirstArc[arc] < ch.Backward.Head.Count);
            Debug.Assert(ch.Backward.ShortcutSecondArc[arc] < ch.Forward.Head.Count);
            UnpackBackwardArc(ch, ch.Backward.ShortcutFirstArc[arc], onNewInputArc);
            UnpackForwardArc(ch, ch.Backward.ShortcutSecondArc[arc], onNewInputArc);
        }
    }

    public List<int> GetNodePath()
    {
        Debug.Assert(ch != null, "Query object must have an attached CH");
        Debug.Assert(state == InternalState.Run, "Invalid state for get_node_path");

        List<int> path = new List<int>();
        if (shortestPathMeetingNode != Constants.INVALID_ID)
        {
            List<int> upPath = new List<int>();
            int x = shortestPathMeetingNode;
            while (forwardPredecessorNode[x] != Constants.INVALID_ID)
            {
                Debug.Assert(wasForwardPushed.IsSet(x));
                upPath.Add(forwardPredecessorArc[x]);
                x = forwardPredecessorNode[x];
            }
            path.Add(ch.Order[x]);

            for (int i = upPath.Count; i > 0; i--)
            {
                UnpackForwardArc(ch, upPath[i - 1], (xy, y) => path.Add(y));
            }

            x = shortestPathMeetingNode;
            while (backwardPredecessorNode[x] != Constants.INVALID_ID)
            {
                Debug.Assert(wasBackwardPushed.IsSet(x));
                UnpackBackwardArc(ch, backwardPredecessorArc[x], (xy, y) => path.Add(y));
                x = backwardPredecessorNode[x];
            }
        }
        return path;
    }

    public ContractionHierarchyQuery PinTargets(List<int> externalTargetList)
    {
        Debug.Assert(ch != null, "Query object must have an attached CH");
        Debug.Assert(externalTargetList.Count == 0 || externalTargetList.Max() < ch.NodeCount, "Node id out of bounds");
        Debug.Assert(state == InternalState.Initialized);

        Pin(
            externalTargetList,
            ch.Rank,

            // The following 4 variables happen to be unused and of the
            // required size -> use them to avoid allocating unnecessary
            // memory. Warning: Usage must be consistent over all pinning functions
            backwardPredecessorNode, out manyToManySourceOrTargetCount, backwardTentativeDistance, ref shortestPathMeetingNode,

            backwardQueue,

            ch.Backward.FirstOut,
            ch.Backward.Head,
            ch.Backward.Weight
        );

        state = InternalState.TargetPinned;
        return this;
    }

    public ContractionHierarchyQuery PinSources(List<int> externalSourceList)
    {
        Debug.Assert(ch != null, "Query object must have an attached CH");
        Debug.Assert(externalSourceList.Count == 0 || externalSourceList.Max() < ch.NodeCount, "Node id out of bounds");
        Debug.Assert(state == InternalState.Initialized);

        Pin(
            externalSourceList,
            ch.Rank,

            forwardPredecessorNode, out manyToManySourceOrTargetCount, forwardTentativeDistance, ref shortestPathMeetingNode,

            forwardQueue,
            ch.Forward.FirstOut,
            ch.Forward.Head,
            ch.Forward.Weight
        );

        state = InternalState.SourcePinned;
        return this;
    }

    public void Pin(
        List<int> externalTargetList,
        int[] externalNodeToInternalNode,
        int[] targetList,
        out int targetCount,
        int[] selectList,
        ref int selectCount,
        MinIDQueue q,
        int[] backwardFirstOut,
        List<int> backwardHead,
        List<int> backwardWeight)
    {
        targetCount = externalTargetList.Count;

        for (int i = 0; i < targetCount; ++i)
        {
            int t = externalTargetList[i];
            t = externalNodeToInternalNode[t];
            targetList[i] = t;
            if (!q.ContainsId(t))
                q.Push(t, t);
        }

        selectCount = 0;
        while (!q.IsEmpty)
        {
            int x = q.Pop().Id;
            selectList[selectCount++] = x;

            for (int xy = backwardFirstOut[x]; xy < backwardFirstOut[x + 1]; ++xy)
            {
                int y = backwardHead[xy];
                Debug.Assert(x < y);
                if (!q.ContainsId(y))
                    q.Push(y, y);
            }
        }

        Array.Reverse(selectList, 0, selectCount);
        //selectList.Reverse(0, selectCount);
    }

}