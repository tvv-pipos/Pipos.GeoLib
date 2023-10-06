using System.Diagnostics;

namespace RoutingKit;

public static class GraphUtils
{
    public static int[] ComputeInverseSortPermutationFirstByLeftThenByRightAndApplySortToLeft(
        int aCount,
        ref List<int> a,
        int bCount,
        List<int> b)
    {
        var p = Sort.ComputeInverseStableSortPermutationUsingKey(b, bCount, x => x);
        a = Permutation.ApplyInversePermutation(p, a);
        var q = Sort.ComputeInverseStableSortPermutationUsingKey(a, aCount, x => x);
        a = Permutation.ApplyInversePermutation(q, a);
        Debug.Assert(Sort.IsSortedUsingLess(a));
        return Permutation.ChainPermutationFirstLeftThenRight(q, p);
    }

    public static int[] ComputeInverseSortPermutationFirstByTailThenByHeadAndApplySortToTail(
        int nodeCount,
        ref List<int> tail,
        List<int> head)
    {
        return ComputeInverseSortPermutationFirstByLeftThenByRightAndApplySortToLeft(nodeCount, ref tail, nodeCount, head);
    }

    public static int[] ComputeInverseSortPermutationFirstByLeftThenByRightAndApplySortToLeft(
        int nodeCount,
        ref List<int> tail,
        List<int> head)
    {
        return ComputeInverseSortPermutationFirstByLeftThenByRightAndApplySortToLeft(nodeCount, ref tail, nodeCount, head);
    }

    // static List<int> ComputeStableSortPermutationUsingKey<T>(List<T> v, int keyCount, Func<T, int> getKey)
    // {
    //     List<int> p = IdentityPermutation(v.Count);
    //     p = Sort.StableSortPermutation(p, (l, r) => getKey(v[l]).CompareTo(getKey(v[r])) > 0);
    //     return p;
    // }

    // static List<int> InvertPermutation(List<int> p)
    // {
    //     List<int> invP = new List<int>(p.Count);
    //     for (int i = 0; i < p.Count; ++i)
    //     {
    //         invP.Add(p.IndexOf(i));
    //     }
    //     return invP;
    // }

    // static List<int> ApplyInversePermutation(List<int> p, List<int> v)
    // {
    //     List<int> r = new List<int>(v.Count);
    //     for (int i = 0; i < v.Count; ++i)
    //     {
    //         r.Add(v[p[i]]);
    //     }
    //     return r;
    // }

    // static void AssertIsSortedUsingLess(List<int> v)
    // {
    //     for (int i = 1; i < v.Count; ++i)
    //     {
    //         if (v[i] < v[i - 1])
    //         {
    //             throw new InvalidOperationException("List is not sorted.");
    //         }
    //     }
    // }

    // static List<int> ChainPermutationFirstLeftThenRight(List<int> p, List<int> q)
    // {
    //     List<int> r = new List<int>(p.Count);
    //     for (int i = 0; i < p.Count; ++i)
    //     {
    //         r.Add(p[q[i]]);
    //     }
    //     return r;
    // }

    // static List<int> IdentityPermutation(int n)
    // {
    //     List<int> p = new List<int>();
    //     for (int i = 0; i < n; ++i)
    //     {
    //         p.Add(i);
    //     }
    //     return p;
    // }

    public static int FindArcGivenSortedHead(int[] firstOut, List<int> head, int x, int y)
    {
        int ret = FindArcOrReturnInvalidGivenSortedHead(firstOut, head, x, y);
        Debug.Assert(ret != Constants.INVALID_ID, "arc does not exist");
        return ret;
    }

    static int counter = 0;
    public static int FindArcOrReturnInvalidGivenSortedHead(int[] firstOut, List<int> head, int x, int y)
    {
        Debug.Assert(x < firstOut.Length - 1, "node id out of bounds");
        Debug.Assert(y < firstOut.Length - 1, "node id out of bounds");
        counter++;
        var range = head.GetRange(firstOut[x], firstOut[x + 1] - firstOut[x]);

        Debug.Assert(range.SequenceEqual(range.OrderBy(i => i)), "heads are not sorted");

        int pos = range.BinarySearch(y);

        if (pos < 0)
            return Constants.INVALID_ID;
        if (range[pos] != y)
            return Constants.INVALID_ID;
        return pos + firstOut[x];
    }

}