using System.Diagnostics;

namespace RoutingKit;

public class Graph
{
    private List<Arc>[] _out;
    private List<Arc>[] _in;
    private int[] _level;

    public Graph(int nodeCount, List<int> tail, List<int> head, List<int> weight)
    {
        _out = new List<Arc>[nodeCount];
        _in = new List<Arc>[nodeCount];
        _level = new int[nodeCount];

        for (var i = 0; i < nodeCount; i++)
        {
            _out[i] = new List<Arc>();
            _in[i] = new List<Arc>();
        }

        for (int a = 0; a < head.Count; a++)
        {
            int x = tail[a];
            int y = head[a];
            int w = weight[a];

            if (x != y)
            {
                _out[x].Add(new Arc { Node = y, Weight = w, HopLength = 1, MidNode = Constants.INVALID_ID });
                _in[y].Add(new Arc { Node = x, Weight = w, HopLength = 1, MidNode = Constants.INVALID_ID });
            }
        }
    }

    public void AddArcOrReduceArcWeight(int x, int midNode, int y, int weight, int hopLength)
    {
        Debug.Assert(x != y);

        Debug.Assert(x < NodeCount);
        Debug.Assert(y < NodeCount);

        Func<int, List<Arc>, int, List<Arc>, bool> reduceArcIfExists = (x1, xOut, y1, yIn) =>
        {
            // Does arc exist?
            for (int outArc = 0; outArc < xOut.Count; outArc++)
            {
                if (xOut[outArc].Node == y1)
                {
                    // Is the existing arc longer?
                    if (xOut[outArc].Weight <= weight)
                        return true;

                    // We need to adjust the weights
                    for (int inArc = 0; inArc < yIn.Count; inArc++)
                    {
                        if (yIn[inArc].Node == x1)
                        {
                            xOut[outArc].Weight = weight;
                            xOut[outArc].HopLength = hopLength;
                            xOut[outArc].MidNode = midNode;
                            yIn[inArc].Weight = weight;
                            yIn[inArc].HopLength = hopLength;
                            yIn[inArc].MidNode = midNode;
                            return true;
                        }
                    }
                    Debug.Assert(false, "arc only exists in one direction");
                }
            }
            return false;
        };

        if (_out[x].Count <= _in[y].Count)
        {
            if (reduceArcIfExists(x, _out[x], y, _in[y]))
                return;
        }
        else
        {
            if (reduceArcIfExists(y, _in[y], x, _out[x]))
                return;
        }

        // The edges does not exist -> add the edge
        _out[x].Add(new Arc { Node = y, Weight = weight, HopLength = hopLength, MidNode = midNode });
        _in[y].Add(new Arc { Node = x, Weight = weight, HopLength = hopLength, MidNode = midNode });
    }

    public void RemoveAllIncidentArcs(int x)
    {
        Debug.Assert(x < NodeCount);

        Action<int, List<Arc>, List<Arc>[]> removeBackArcs = (x1, xOut, in1) =>
        {
            for (int outArc = 0; outArc < xOut.Count; outArc++)
            {
                int y = xOut[outArc].Node;
                for (int inArc = 0; ; inArc++)
                {
                    Debug.Assert(inArc < in1[y].Count);
                    if (in1[y][inArc].Node == x1)
                    {
                        in1[y].RemoveAt(inArc);
                        break;
                    }
                }
            }
        };

        removeBackArcs(x, _out[x], _in);
        removeBackArcs(x, _in[x], _out);

        _in[x].Clear();
        _out[x].Clear();
        _in[x].TrimExcess();
        _out[x].TrimExcess();
    }



    public int OutDeg(int node)
    {
        Debug.Assert(node < NodeCount);
        return _out[node].Count;
    }

    public int InDeg(int node)
    {
        Debug.Assert(node < NodeCount);
        return _in[node].Count;
    }

    public Arc Out(int node, int outArc)
    {
        Debug.Assert(node < NodeCount);
        Debug.Assert(outArc < _out[node].Count);
        return _out[node][outArc];
    }

    public Arc In(int node, int inArc)
    {
        Debug.Assert(node < NodeCount);
        Debug.Assert(inArc < _in[node].Count);
        return _in[node][inArc];
    }

    public int NodeCount { get {
        Debug.Assert(_in.Length == _out.Length);
        return _out.Length;
    }} 

    public int Level(int node)
    {
        Debug.Assert(node < NodeCount);
        return _level[node];
    }

    public void RaiseLevel(int node, int level)
    {
        Debug.Assert(node < NodeCount);
        if (level >= _level[node])
            _level[node] = level;
    }

}