using System.Collections;
using System.Diagnostics;

namespace RoutingKit;

static class ContractionHierarchyHelpers
{
    public static void SortChArcsAndBuildFirstOutArrays(
        ContractionHierarchy ch,
        ContractionHierarchyExtraInfo chExtra,
        Action<string>? logMessage)
    {
        long timer = 0; // initialize to avoid warning, not needed
        if (logMessage != null)
        {
            timer = -Timer.GetMicroTime();
            logMessage("Start sorting arcs.");
        }

        int nodeCount = ch.Rank.Length;

        {
            var tail = chExtra.Forward.Tail;
            var r = GraphUtils.ComputeInverseSortPermutationFirstByTailThenByHeadAndApplySortToTail(nodeCount, ref tail, ch.Forward.Head);
            chExtra.Forward.Tail = tail;

            ch.Forward.Head = Permutation.ApplyInversePermutation(r, ch.Forward.Head);
            ch.Forward.Weight = Permutation.ApplyInversePermutation(r, ch.Forward.Weight);
            chExtra.Forward.MidNode = Permutation.ApplyInversePermutation(r, chExtra.Forward.MidNode);

            ch.Forward.FirstOut = InvVecUtils.InvertVector(chExtra.Forward.Tail, nodeCount);
        }

        {
            var tail = chExtra.Backward.Tail;
            var r = GraphUtils.ComputeInverseSortPermutationFirstByTailThenByHeadAndApplySortToTail(nodeCount, ref tail, ch.Backward.Head);
            chExtra.Backward.Tail = tail;

            ch.Backward.Head = Permutation.ApplyInversePermutation(r, ch.Backward.Head);
            ch.Backward.Weight = Permutation.ApplyInversePermutation(r, ch.Backward.Weight);
            chExtra.Backward.MidNode = Permutation.ApplyInversePermutation(r, chExtra.Backward.MidNode);

            ch.Backward.FirstOut = InvVecUtils.InvertVector(chExtra.Backward.Tail, nodeCount);
        }

        if (logMessage != null)
        {
            timer += Timer.GetMicroTime();
            logMessage("Finished sorting arcs. Needed " + timer + " musec.");
        }
    }

    public static void SortArcsAndRemoveMultiAndLoopArcs(
        int nodeCount,
        ref List<int> tail,
        ref List<int> head,
        ref List<int> weight,
        ref List<int> inputArcId,
        Action<string>? logMessage)
    {
        long timer = 0; // initialize to avoid warning, not needed
        if (logMessage != null)
        {
            timer = -Timer.GetMicroTime();
            logMessage("Start removing loops and multi arcs from input.");
        }

        {
            var p = GraphUtils.ComputeInverseSortPermutationFirstByLeftThenByRightAndApplySortToLeft(nodeCount, ref tail, head);
            head = Permutation.ApplyInversePermutation(p, head);
            weight = Permutation.ApplyInversePermutation(p, weight);
            inputArcId = Permutation.ApplyInversePermutation(p, inputArcId);
        }

        int arcCount = head.Count;

        if (arcCount != 0)
        {
            int outCount = 0;
            for (int inCount = 0; inCount < arcCount; ++inCount)
            {
                if (tail[inCount] != head[inCount])
                {
                    tail[outCount] = tail[inCount];
                    head[outCount] = head[inCount];
                    weight[outCount] = weight[inCount];
                    inputArcId[outCount] = inputArcId[inCount];
                    ++outCount;
                }
            }
            arcCount = outCount;
        }

        if (arcCount != 0)
        {
            int outCount = 1;
            for (int inCount = 1; inCount < arcCount; ++inCount)
            {
                if (tail[(inCount - 1)] != tail[inCount] || head[(inCount - 1)] != head[inCount])
                {
                    tail[outCount] = tail[inCount];
                    head[outCount] = head[inCount];
                    weight[outCount] = weight[inCount];
                    inputArcId[outCount] = inputArcId[inCount];
                    ++outCount;
                }
                else
                {
                    if (weight[inCount] < weight[(outCount - 1)])
                    {
                        weight[(outCount - 1)] = weight[inCount];
                        inputArcId[(outCount - 1)] = inputArcId[inCount];
                    }
                }
            }
            arcCount = outCount;
        }

        tail.RemoveRange(arcCount, tail.Count - arcCount);
        head.RemoveRange(arcCount, head.Count - arcCount);
        weight.RemoveRange(arcCount, weight.Count - arcCount);
        inputArcId.RemoveRange(arcCount, inputArcId.Count - arcCount);

        if (logMessage != null)
        {
            timer += Timer.GetMicroTime();
            logMessage($"Finished removing loops and multi arcs from input. Needed {timer}musec time.");
        }
    }

    static int EstimateNodeImportance(Graph graph, ShorterPathTest shorterPathTest, int node)
    {
    	if(node == 8)
		{
			int t = node;
		}
        int level = graph.Level(node);
        int addedArcCount = 0;
        int addedHopCount = 0;

        for (int inArc = 0; inArc < graph.InDeg(node); ++inArc)
        {
            int inNode = graph.In(node, inArc).Node;
            shorterPathTest.PinSource(inNode, node);

            for (int outArc = 0; outArc < graph.OutDeg(node); ++outArc)
            {
                int outNode = graph.Out(node, outArc).Node;

                if (inNode != outNode)
                {
                    if (!shorterPathTest.DoesShorterOrEqualPathToTargetExist(outNode, graph.In(node, inArc).Weight + graph.Out(node, outArc).Weight))
                    {
                        ++addedArcCount;
                        addedHopCount += graph.In(node, inArc).HopLength;
                        addedHopCount += graph.Out(node, outArc).HopLength;
                    }
                }
            }
        }

        int removedArcCount = 1;
        removedArcCount += graph.InDeg(node);
        removedArcCount += graph.OutDeg(node);

        int removedHopCount = 1;
        for (int inArc = 0; inArc < graph.InDeg(node); ++inArc)
        {
            removedHopCount += graph.In(node, inArc).HopLength;
        }

        for (int outArc = 0; outArc < graph.OutDeg(node); ++outArc)
        {
            removedHopCount += graph.Out(node, outArc).HopLength;
        }

        return 1 + 1000 * level + (1000 * addedArcCount) / removedArcCount + (1000 * addedHopCount) / removedHopCount;
    }

    static void ContractNode(Graph graph, ShorterPathTest shorterPathTest, int nodeBeingContracted)
    {
        for (int inArc = 0; inArc < graph.InDeg(nodeBeingContracted); ++inArc)
        {
            int inNode = graph.In(nodeBeingContracted, inArc).Node;
            shorterPathTest.PinSource(inNode, nodeBeingContracted);
            for (int outArc = 0; outArc < graph.OutDeg(nodeBeingContracted); ++outArc)
            {
                int outNode = graph.Out(nodeBeingContracted, outArc).Node;
                if (inNode != outNode)
                {
                    if (!shorterPathTest.DoesShorterOrEqualPathToTargetExist(outNode,
                        graph.In(nodeBeingContracted, inArc).Weight + graph.Out(nodeBeingContracted, outArc).Weight))
                    {
                        graph.AddArcOrReduceArcWeight(
                            inNode, nodeBeingContracted, outNode,
                            graph.In(nodeBeingContracted, inArc).Weight + graph.Out(nodeBeingContracted, outArc).Weight,
                            graph.In(nodeBeingContracted, inArc).HopLength + graph.Out(nodeBeingContracted, outArc).HopLength);
                    }
                }
            }
        }

        graph.RemoveAllIncidentArcs(nodeBeingContracted);

        Debug.Assert(graph.OutDeg(nodeBeingContracted) == 0);
        Debug.Assert(graph.InDeg(nodeBeingContracted) == 0);
    }

    public static void BuildChAndOrder(
        Graph graph,
        ContractionHierarchy ch,
        ContractionHierarchyExtraInfo chExtra,
        int maxPopCount,
        Action<string>? logMessage)
    {
        long timer = 0;  // initialize to avoid warning, not needed
        long lastLogMessageTime = 0;  // initialize to avoid warning, not needed
        if (logMessage != null)
        {
            lastLogMessageTime = Timer.GetMicroTime();
            timer = -lastLogMessageTime;
            logMessage("Start building queue.");
        }

        int nodeCount = graph.NodeCount;

        ShorterPathTest shorterPathTest = new ShorterPathTest(graph, maxPopCount);

        ch.ResizeRank(nodeCount);
        ch.ResizeOrder(nodeCount);
        MinIDQueue queue = new MinIDQueue(nodeCount);

        for (int i = 0; i < nodeCount; ++i)
        {
            int q = EstimateNodeImportance(graph, shorterPathTest, i);
            queue.Push(i, q);
            //System.Console.WriteLine(i + " " + q);
            if (logMessage != null)
            {
                long currentTime = Timer.GetMicroTime();
                if (currentTime - lastLogMessageTime > 1000000)
                {
                    lastLogMessageTime = currentTime;
                    logMessage($"Added {i + 1} of {nodeCount} nodes to the queue. Running for {timer + currentTime} musec.");
                }
            }
			/*if((i+1) % 100 == 0)
			{
				System.Console.ReadKey();
			}*/
        }

        if (logMessage != null)
        {
            timer += Timer.GetMicroTime();
            logMessage($"Finished building queue. Needed {timer} musec time.");
            logMessage("Start contracting nodes.");
            timer = -Timer.GetMicroTime();
        }

        List<int> neighborList = new List<int>();
        bool[] isNeighbor = new bool[nodeCount];
        for (var i = 0; i < nodeCount; i++)
        {
            isNeighbor[i] = false;
        }

        int contractedNodeCount = 0;


        var sw = Stopwatch.StartNew();
        var lastTime = sw.ElapsedMilliseconds;

        var counter = 0;

        while (!queue.IsEmpty)
        {
            counter++;
            int nodeBeingContracted = queue.Pop().Id;

            ch.Rank[nodeBeingContracted] = contractedNodeCount;
            ch.Order[contractedNodeCount] = nodeBeingContracted;

            // Mark the neighbors
            for (int inArc = 0; inArc < graph.InDeg(nodeBeingContracted); ++inArc)
            {
                int x = graph.In(nodeBeingContracted, inArc).Node;
                Debug.Assert(nodeBeingContracted != x);
                if (!isNeighbor[x])
                {
                    neighborList.Add(x);
                    isNeighbor[x] = true;
                }
            }

            for (int outArc = 0; outArc < graph.OutDeg(nodeBeingContracted); ++outArc)
            {
                int x = graph.Out(nodeBeingContracted, outArc).Node;
                Debug.Assert(nodeBeingContracted != x);
                if (!isNeighbor[x])
                {
                    neighborList.Add(x);
                    isNeighbor[x] = true;
                }
            }

            // Add the arcs to the search graph
            for (int outArc = 0; outArc < graph.OutDeg(nodeBeingContracted); ++outArc)
            {
                chExtra.Forward.Tail.Add(nodeBeingContracted);

                var a = graph.Out(nodeBeingContracted, outArc);
                if (ch.Forward.Head.Count == Constants.INVALID_ID)
                {
                    throw new InvalidOperationException("CH may contain at most 2^32-1 shortcuts per direction");
                }

                ch.Forward.Head.Add(a.Node);
                ch.Forward.Weight.Add(a.Weight);
                chExtra.Forward.MidNode.Add(a.MidNode);
            }

            for (int inArc = 0; inArc < graph.InDeg(nodeBeingContracted); ++inArc)
            {
                chExtra.Backward.Tail.Add(nodeBeingContracted);

                var a = graph.In(nodeBeingContracted, inArc);
                if (ch.Backward.Head.Count == Constants.INVALID_ID)
                {
                    throw new InvalidOperationException("CH may contain at most 2^32-1 shortcuts per direction");
                }

                ch.Backward.Head.Add(a.Node);
                ch.Backward.Weight.Add(a.Weight);
                chExtra.Backward.MidNode.Add(a.MidNode);
            }

            int neighborLevel = graph.Level(nodeBeingContracted) + 1;
            int outDeg = graph.OutDeg(nodeBeingContracted);
            int inDeg = graph.InDeg(nodeBeingContracted);

            ContractNode(graph, shorterPathTest, nodeBeingContracted);

            foreach (var x in neighborList)
            {
                isNeighbor[x] = false;
                graph.RaiseLevel(x, neighborLevel);
                int newKey = EstimateNodeImportance(graph, shorterPathTest, x);
                Debug.Assert(queue.ContainsId(x));
                int oldKey = queue.GetKey(x);
                if (oldKey < newKey)
                {
                    queue.IncreaseKey(x, newKey);
                }
                else if (oldKey > newKey)
                {
                    queue.DecreaseKey(x, newKey);
                }
            }

            neighborList.Clear();

            ++contractedNodeCount;

            if (logMessage != null)
            {
                if (sw.ElapsedMilliseconds - lastTime > 1000)
                {

                    lastTime = sw.ElapsedMilliseconds;
                    logMessage($"Contracted {contractedNodeCount} of {nodeCount}. The in degree of the last node was {inDeg} and out degree was {outDeg}, processed {counter} nodes ({ShorterPathTest.counter/counter} tpn)");
                    counter = 0;
                    ShorterPathTest.counter = 0;
                }
                // long currentTime = Timer.GetMicroTime();
                // if (currentTime - lastLogMessageTime > 1000000)
                // {
                //     lastLogMessageTime = currentTime;
                //     logMessage($"Contracted {contractedNodeCount} of {nodeCount}. The in degree of the last node was {inDeg} and out degree was {outDeg}. Running for {timer + currentTime} musec.");
                // }
            }
        }

        ch.Forward.Head.TrimExcess();
        ch.Forward.Weight.TrimExcess();
        chExtra.Forward.MidNode.TrimExcess();
        chExtra.Forward.Tail.TrimExcess();

        ch.Backward.Head.TrimExcess();
        ch.Backward.Weight.TrimExcess();
        chExtra.Backward.MidNode.TrimExcess();
        chExtra.Backward.Tail.TrimExcess();

        if (logMessage != null)
        {
            timer += Timer.GetMicroTime();
            logMessage($"Finished contracting nodes. Needed {timer} musec.");
        }
    }

    public static void LogInputGraphStatistics(
        int nodeCount,
        List<int> tail,
        List<int> head,
        Action<string>? logMessage)
    {
        if (logMessage != null)
        {
            logMessage($"Input graph has {nodeCount} nodes and {tail.Count} arcs.");
            int[] deg = new int[nodeCount];

            for (int i = 0; i < tail.Count; ++i)
            {
                deg[tail[i]]++;
            }

            int maxOutDeg = MinMax.MaxElementOf(deg);
            deg = new int[nodeCount];

            for (int i = 0; i < tail.Count; ++i)
            {
                deg[head[i]]++;
            }

            int maxInDeg = MinMax.MaxElementOf(deg);
            logMessage($"The input's maximum in-degree is {maxInDeg} and its maximum out-degree is {maxOutDeg}.");
        }
    }

    public static void OptimizeOrderForCache(ContractionHierarchy ch, ContractionHierarchyExtraInfo chExtra, Action<string>? logMessage)
    {
        long timer = 0; // initialize to avoid warning, not needed
        if (logMessage != null)
        {
            timer = -Timer.GetMicroTime();
            logMessage("Start optimizing order for cache.");
        }

        int nodeCount = ch.Rank.Length;
        int forwardArcCount = ch.Forward.Head.Count;
        int backwardArcCount = ch.Backward.Head.Count;

        bool[] isInNewOrder = new bool[nodeCount];
        bool[] isInBottomLevel = new bool[nodeCount];
        int[] newOrder = new int[nodeCount];
        for (var i = 0; i < nodeCount; i++)
        {
            isInBottomLevel[i] = true;
            isInNewOrder[i] = false;
         }
           
        for (int a = 0; a < forwardArcCount; ++a)
            isInBottomLevel[ch.Forward.Head[a]] = false;
        for (int a = 0; a < backwardArcCount; ++a)
            isInBottomLevel[ch.Backward.Head[a]] = false;
        
        int newOrderEnd = nodeCount;

        MinIDQueue q = new MinIDQueue(nodeCount);
        for (int r = 0; r < nodeCount; ++r)
        {
            if (isInBottomLevel[r])
            {
                int searchSpaceEnd = newOrderEnd;

                q.Push(r, ch.Rank[r]);
                Debug.Assert(!isInNewOrder[r]);
                isInNewOrder[r] = true;

                while (!q.IsEmpty)
                {
                    int x = q.Pop().Id;
                    
                    --newOrderEnd;
                    newOrder[newOrderEnd] = x;

                    Action<int> onNode = (int y) =>
                    {
                        Debug.Assert(!isInBottomLevel[y]);
                        if (!isInNewOrder[y])
                        {
                            isInNewOrder[y] = true;
                            q.Push(y, ch.Rank[y]);
                        }
                    };

                    for (int xy = ch.Forward.FirstOut[x]; xy < ch.Forward.FirstOut[x + 1]; ++xy)
                        onNode(ch.Forward.Head[xy]);

                    for (int xy = ch.Backward.FirstOut[x]; xy < ch.Backward.FirstOut[x + 1]; ++xy)
                        onNode(ch.Backward.Head[xy]);
                }
                Array.Reverse(newOrder, newOrderEnd, searchSpaceEnd - newOrderEnd);
            }
        }

        Debug.Assert(newOrderEnd == 0);
        Debug.Assert(Permutation.IsPermutation(newOrder));

        ch.Rank = Permutation.InvertPermutation(newOrder);
        ch.Order = newOrder;

        if (logMessage != null)
        {
            timer += Timer.GetMicroTime();
            logMessage("Finished optimizing order for cache. Needed " + timer + "musec.");
        }
    }


    public static void MakeInternalNodesAndRankCoincide(ContractionHierarchy ch,
        ContractionHierarchyExtraInfo chExtra, Action<string>? logMessage)
    {
        long timer = 0; // initialize to avoid warning, not needed
        if (logMessage != null)
        {
            timer = -Timer.GetMicroTime();
            logMessage("Start reordering nodes by rank.");
        }

        Permutation.InplaceApplyPermutationToElementsOf(ch.Rank, ch.Forward.Head);
        Permutation.InplaceApplyPermutationToElementsOf(ch.Rank, chExtra.Forward.Tail);
        Permutation.InplaceApplyPermutationToPossiblyInvalidElementsOf(ch.Rank, chExtra.Forward.MidNode);
        Permutation.InplaceApplyPermutationToElementsOf(ch.Rank, ch.Backward.Head);
        Permutation.InplaceApplyPermutationToElementsOf(ch.Rank, chExtra.Backward.Tail);
        Permutation.InplaceApplyPermutationToPossiblyInvalidElementsOf(ch.Rank, chExtra.Backward.MidNode);

#if !DEBUG
    for (int i = 0; i < chExtra.Forward.Tail.Count; ++i)
        Debug.Assert(chExtra.Forward.Tail[i] < ch.Forward.Head[i]);
    for (int i = 0; i < chExtra.Backward.Tail.Count; ++i)
        Debug.Assert(chExtra.Backward.Tail[i] < ch.Backward.Head[i]);
#endif

        if (logMessage != null)
        {
            timer += Timer.GetMicroTime();
            logMessage("Finished reordering nodes by rank. Needed " + timer + "musec.");
        }
    }

    public static void BuildUnpackingInformation(
        int nodeCount,
        List<int> tail,
        List<int> head,
        List<int> inputArcId,
        ContractionHierarchy ch,
        ContractionHierarchyExtraInfo chExtra,
        Action<string>? logMessage)
    {
        Debug.Assert(Sort.IsSortedUsingLess(tail));

        long timer = 0;  // initialize to avoid warning, not needed
        if (logMessage != null)
        {
            logMessage("Start building path unpacking information.");
            timer = -Timer.GetMicroTime();
        }

        ch.Forward.ShortcutFirstArc = new int[ch.Forward.Head.Count];
        ch.Forward.ShortcutSecondArc = new int[ch.Forward.Head.Count];
        ch.Forward.IsShortcutAnOriginalArc = new BitArray(ch.Forward.Head.Count);
        ch.Backward.ShortcutFirstArc = new int[ch.Backward.Head.Count];
        ch.Backward.ShortcutSecondArc = new int[ch.Backward.Head.Count];
        ch.Backward.IsShortcutAnOriginalArc = new BitArray(ch.Backward.Head.Count);

        int[] firstOut = InvVecUtils.InvertVector(tail, nodeCount);

        for (int x = 0; x < nodeCount; ++x)
        {
            for (int xy = ch.Forward.FirstOut[x]; xy < ch.Forward.FirstOut[x + 1]; ++xy)
            {
                int y = ch.Forward.Head[xy];
                int z = chExtra.Forward.MidNode[xy];
                if (z == Constants.INVALID_ID)
                {
                    ch.Forward.IsShortcutAnOriginalArc.Set(xy, true);

                    int a = GraphUtils.FindArcGivenSortedHead(firstOut, head, ch.Order[x], ch.Order[y]);
                    ch.Forward.ShortcutFirstArc[xy] = inputArcId[a];
                    ch.Forward.ShortcutSecondArc[xy] = head[a];
                }
                else
                {
                    ch.Forward.IsShortcutAnOriginalArc.Set(xy, false);
                    ch.Forward.ShortcutFirstArc[xy] = GraphUtils.FindArcGivenSortedHead(ch.Backward.FirstOut, ch.Backward.Head, z, x);
                    ch.Forward.ShortcutSecondArc[xy] = GraphUtils.FindArcGivenSortedHead(ch.Forward.FirstOut, ch.Forward.Head, z, y);

                    Debug.Assert(ch.Forward.Weight[xy] == ch.Backward.Weight[ch.Forward.ShortcutFirstArc[xy]] + ch.Forward.Weight[ch.Forward.ShortcutSecondArc[xy]]);
                }
            }
        }

        for (int x = 0; x < nodeCount; ++x)
        {
            for (int xy = ch.Backward.FirstOut[x]; xy < ch.Backward.FirstOut[x + 1]; ++xy)
            {
                int y = ch.Backward.Head[xy];
                int z = chExtra.Backward.MidNode[xy];
                if (z == Constants.INVALID_ID)
                {
                    ch.Backward.IsShortcutAnOriginalArc.Set(xy, true);
                    int a = GraphUtils.FindArcGivenSortedHead(firstOut, head, ch.Order[y], ch.Order[x]);
                    ch.Backward.ShortcutFirstArc[xy] = inputArcId[a];
                    ch.Backward.ShortcutSecondArc[xy] = head[a];
                }
                else
                {
                    ch.Backward.IsShortcutAnOriginalArc.Set(xy, false);
                    ch.Backward.ShortcutFirstArc[xy] = GraphUtils.FindArcGivenSortedHead(ch.Backward.FirstOut, ch.Backward.Head, z, y);
                    ch.Backward.ShortcutSecondArc[xy] = GraphUtils.FindArcGivenSortedHead(ch.Forward.FirstOut, ch.Forward.Head, z, x);
                }
            }
        }

#if DEBUG
    int inputArcCount = MinMax.MaxElementOf(inputArcId, 0) + 1;

    for (int a = 0; a < ch.Forward.Head.Count; ++a)
    {
        if (!ch.Forward.IsShortcutAnOriginalArc[a])
        {
            Debug.Assert(ch.Forward.ShortcutFirstArc[a] < ch.Backward.Head.Count);
            Debug.Assert(ch.Forward.ShortcutSecondArc[a] < ch.Forward.Head.Count);
        }
        else
        {
            Debug.Assert(ch.Forward.ShortcutFirstArc[a] < inputArcCount);
            Debug.Assert(ch.Forward.ShortcutSecondArc[a] < nodeCount);
        }
    }
    for (int a = 0; a < ch.Backward.Head.Count; ++a)
    {
        if (!ch.Backward.IsShortcutAnOriginalArc[a])
        {
            Debug.Assert(ch.Backward.ShortcutFirstArc[a] < ch.Backward.Head.Count);
            Debug.Assert(ch.Backward.ShortcutSecondArc[a] < ch.Forward.Head.Count);
        }
        else
        {
            Debug.Assert(ch.Backward.ShortcutFirstArc[a] < inputArcCount);
            Debug.Assert(ch.Backward.ShortcutSecondArc[a] < nodeCount);
        }
    }
#endif

        if (logMessage != null)
        {
            timer += Timer.GetMicroTime();
            logMessage("Finished building path unpacking information. Needed " + timer + "musec.");
            logMessage("Contraction Hierarchy is fully constructed.");
        }
    }

    static void ResizeList<T>(List<T> list, int newSize, T defaultValue)
    {
        if (newSize < 0)
        {
            throw new ArgumentException("Storlek måste vara ett icke-negativt heltal.", nameof(newSize));
        }

        int currentSize = list.Count;

        if (newSize < currentSize)
        {
            list.RemoveRange(newSize, currentSize - newSize);
        }
        else if (newSize > currentSize)
        {
            int elementsToAdd = newSize - currentSize;
            list.AddRange(new T[elementsToAdd]);
            for (int i = currentSize; i < newSize; i++)
            {
                list[i] = defaultValue;
            }
        }
    }

    static void PrintList<T>(List<T> list)
    {
        foreach (var item in list)
        {
            Console.Write(item + " ");
        }
        Console.WriteLine();
    }
}