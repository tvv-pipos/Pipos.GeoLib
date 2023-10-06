using System.Diagnostics;

namespace RoutingKit;

public static class InvVecUtils
{
    
    // The inverse vector p of a vector v is a vector such that the elements
    // v[p[i]], v[p[i]+1], v[p[i]+2], ..., v[p[i+1]-1] are exactly the elements
    // with value i in v. If i does not occur in v, then p[i] == p[i+1]. v must be
    // a sorted vector of unsigned integers.
    public static int[] InvertVector(int[] v, int elementCount)
    {
        var index = new int[elementCount + 1];

        if (v.Length == 0)
        {
            for (int i = 0; i <= index.Length; i++)
            {
                index[i] = 0;
            }
        }
        else
        {
            Debug.Assert(Sort.IsSortedUsingLess(v));
            Debug.Assert(MinMax.MaxElementOf(v) < elementCount);

            index[0] = 0;
            int pos = 0;

            for (int i = 0; i < elementCount; i++)
            {
                while (pos < v.Length && v[pos] < i)
                {
                    pos++;
                }

                index[i] = pos;
            }

            index[elementCount] = v.Length;
        }

        return index;
    }

    public static int[] InvertVector(List<int> v, int elementCount)
    {
        var index = new int[elementCount + 1];

        if (v.Count == 0)
        {
            for (int i = 0; i <= index.Length; i++)
            {
                index[i] = 0;
            }
        }
        else
        {
            Debug.Assert(Sort.IsSortedUsingLess(v));
            Debug.Assert(MinMax.MaxElementOf(v) < elementCount);

            index[0] = 0;
            int pos = 0;

            for (int i = 0; i < elementCount; i++)
            {
                while (pos < v.Count && v[pos] < i)
                {
                    pos++;
                }

                index[i] = pos;
            }

            index[elementCount] = v.Count;
        }

        return index;
    }

    public static List<int> InvertInverseVector(List<int> sortedIndex)
    {
        if (sortedIndex.Count == 0)
        {
            // Handle the case where the input is empty.
            // You can add error handling or take appropriate action here.
            return new List<int>();
        }

        var v = new List<int>(new int[sortedIndex.Last()]);

        for (int i = 0; i < sortedIndex.Count - 1; i++)
        {
            for (int j = sortedIndex[i]; j < sortedIndex[i + 1]; j++)
            {
                v[j] = i;
            }
        }

        return v;
    }
}