namespace RoutingKit;

public static class Sort
{
    private const int BucketSortMinKeyToElementRatio = 512;

    class SortComparer<T> : IComparer<T>
    {
        private readonly Func<T?, T?, int> _compare;

        public SortComparer(Func<T?, T?, int> compare)
        {
            _compare = compare;
        }

        public int Compare(T? left, T? right)
        {
            return _compare(left, right);
        }
    }

    class StableSortComparer<T> : IComparer<T>
    {
        private readonly Func<T?, T?, int> _compare;

        public StableSortComparer(Func<T?, T?, int> compare)
        {
            _compare = compare;
        }

        public int Compare(T? left, T? right)
        {
            return _compare(left, right);
        }
    }

    public static int[] ComputeStableSortPermutationUsingComparator<T>(List<T> v, Func<T, T, int> isLess)
    {
        Func<int, int, int> c = (l, r) => isLess(v[l], v[r]);
        int[] p = Permutation.IdentityPermutation(v.Count);
        Timsort.Sort(p, new StableSortComparer<int>(c));
        return p;
    }

    // public static int[] StableSortPermutation(int[] p, Func<int, int, int> compare)
    // {
    //     var comparer = new StableSortComparer<int>(compare);
    //     return  Timsort.Sort(p, comparer);//Array.Sort() p.OrderBy(x => x, new StableSortComparer<int>(compare));
    // }

    // public static List<T> StableSortUsingComparatorInplace<T>(List<T> v, Func<T, T, int> isLess)
    // {
    //     v.Sort(new StableSortComparer<T>(isLess));
    //     return v;
    // }

    // public static List<T> StableSortUsingComparator<T>(List<T> v, Func<T, T, int> isLess)
    // {
    //     List<T> r = new List<T>(v);
    //     r.Sort(new StableSortComparer<T>(isLess));
    //     return r;
    // }

    public static int[] ComputeInverseStableSortPermutationUsingLess<T>(List<T> v) where T : IComparable<T>
    {
        return Permutation.InvertPermutation(ComputeStableSortPermutationUsingLess(v));
    }

      public static int[] ComputeStableSortPermutationUsingLess<T>(List<T> v) where T : IComparable<T>
    {
        return ComputeStableSortPermutationUsingComparator(v, (l, r) => l.CompareTo(r));
    }

    public static int[] ComputeSortPermutationUsingComparator<T>(List<T> v, Func<T, T, int> isLess)
    {
        var p = Permutation.IdentityPermutation(v.Count);
        var comparer = new SortComparer<int>((l, r) => isLess(v[l], v[r]));
        Array.Sort(p, comparer);
        return p;
    }

    // public static List<int> SortPermutation(List<int> p, Func<int, int, int> compare)
    // {
    //     return p.OrderBy(x => x, new SortComparer<int>(compare)).ToList();
    // }

    // public static List<T> SortUsingComparator<T>(List<T> v, Func<T, T, int> isLess)
    // {
    //     List<T> r = new List<T>(v);
    //     r.Sort(new SortComparer<T>(isLess));
    //     return r;
    // }

    // public static List<T> SortUsingComparatorInplace<T>(List<T> v, Func<T, T, int> isLess)
    // {
    //     v.Sort(new SortComparer<T>(isLess));
    //     return v;
    // }

    public static List<int> IdentityPermutation(int n)
    {
        List<int> p = new List<int>();
        for (int i = 0; i < n; ++i)
        {
            p.Add(i);
        }
        return p;
    }

    public static bool IsSortedUsingComparator<T>(List<T> v, Func<T, T, bool> isLess)
    {
        for (int i = 1; i < v.Count; ++i)
        {
            if (isLess(v[i], v[i - 1]))
            {
                return false;
            }
        }
        return true;
    }

    public static bool IsSortedUsingComparator<T>(T[] v, Func<T, T, bool> isLess)
    {
        for (int i = 1; i < v.Length; ++i)
        {
            if (isLess(v[i], v[i - 1]))
            {
                return false;
            }
        }
        return true;
    }

    public static int[] ComputeInverseSortPermutationUsingLess<T>(List<T> v) where T : IComparable<T>
    {
        var sortPermutation = ComputeSortPermutationUsingComparator(v, (l, r) => l.CompareTo(r));
        return Permutation.InvertPermutation(sortPermutation);
    }

    public static int[] ComputeInverseSortPermutationUsingComparator<T>(List<T> v, Func<T, T, int> isLess)
    {
        var sortPermutation = ComputeSortPermutationUsingComparator(v, isLess);
        return Permutation.InvertPermutation(sortPermutation);
    }

    public static int[] ComputeInverseStableSortPermutationUsingComparator<T>(List<T> v, Func<T, T, int> isLess)
    {
        int[] stableSortPermutation = ComputeStableSortPermutationUsingComparator(v, isLess);
        return Permutation.InvertPermutation(stableSortPermutation);
    }

    public static int[] ComputeSortPermutationUsingLess<T>(List<T> v) where T : IComparable<T>
    {
        return ComputeSortPermutationUsingComparator<T>(v, (l, r) => l.CompareTo(r));
    }

    public static int[] ComputeSortPermutationUsingKey<T>(List<T> v, int keyCount, Func<T, int> getKey)
    {
        return ComputeMaybeStableSortPermutationUsingKey<T>(false, v, keyCount, getKey);
    }

    public static int[] ComputeStableSortPermutationUsingKey<T>(List<T> v, int keyCount, Func<T, int> getKey)
    {
        return ComputeMaybeStableSortPermutationUsingKey<T>(true, v, keyCount, getKey);
    }

    public static int[] ComputeMaybeStableSortPermutationUsingKey<T>(bool isStable, List<T> v, int keyCount, Func<T, int> getKey)
    {
        int[] p;

        if (v.Count >= keyCount / BucketSortMinKeyToElementRatio)
        {
            p = new int[v.Count];
            var keyPos = ComputeKeyPos(v, keyCount, getKey);
            for (int i = 0; i < v.Count; i++)
            {
                int k = getKey(v[i]);
                if (k > keyCount)
                {
                    throw new Exception("Key is too large.");
                }
                p[keyPos[k]] = i;
                keyPos[k]++;
            }
        }
        else if (isStable)
        {
            p = ComputeStableSortPermutationUsingComparator(v, MakeCompareByKey(keyCount, getKey));
        }
        else
        {
            p = ComputeSortPermutationUsingComparator(v, MakeCompareByKey(keyCount, getKey));
        }

        return p;
    }

    public static int[] ComputeInverseSortPermutationUsingKey<T>(List<T> v, int keyCount, Func<T, int> getKey)
    {
        return ComputeInverseMaybeStableSortPermutationUsingKey(false, v, keyCount, getKey);
    }

    public static int[] ComputeInverseStableSortPermutationUsingKey<T>(List<T> v, int keyCount, Func<T, int> getKey)
    {
        return ComputeInverseMaybeStableSortPermutationUsingKey(true, v, keyCount, getKey);
    }

    public static int[] ComputeInverseMaybeStableSortPermutationUsingKey<T>(bool isStable, List<T> v, int keyCount, Func<T, int> getKey)
    {
        int[] p;

        if (v.Count >= keyCount / BucketSortMinKeyToElementRatio)
        {
            p = new int[v.Count];
            var keyPos = ComputeKeyPos(v, keyCount, getKey);

            for (int i = 0; i < v.Count; ++i)
            {
                int k = getKey(v[i]);
                if (k > keyCount)
                {
                    throw new ArgumentException("Key is too large.");
                }
                p[i] = keyPos[k];
                keyPos[k]++;
            }
        }
        else
        {
            if (isStable)
            {
                p = ComputeInverseStableSortPermutationUsingComparator(v, MakeCompareByKey(keyCount, getKey));
            }
            else
            {
                p = ComputeInverseSortPermutationUsingComparator(v, MakeCompareByKey(keyCount, getKey));
            }
        }

        return p;
    }

    private static Func<T, T, int> MakeCompareByKey<T>(int keyCount, Func<T, int> getKey)
    {
        return (t, k) =>
        {
            var k1 = getKey(t);
            var k2 = getKey(k);
            return k1.CompareTo(k2);
        };
    }

    public static int[] ComputeKeyPos<T>(List<T> v, int keyCount, Func<T, int> getKey)
    {
        var keyPos = new int[keyCount];

        for (int i = 0; i < v.Count; i++)
        {
            int k = getKey(v[i]);
            if (k > keyCount)
            {
                throw new Exception("Key is too large.");
            }
            keyPos[k]++;
        }

        int sum = 0;
        for (int i = 0; i < keyCount; i++)
        {
            int tmp = sum + keyPos[i];
            keyPos[i] = sum;
            sum = tmp;
        }

        return keyPos;
    }



    public static bool IsSortedUsingLess<T>(List<T> v) where T : IComparable<T>
    {
        return IsSortedUsingComparator(v, (l, r) => l.CompareTo(r) < 0);
    }

    public static bool IsSortedUsingLess<T>(T[] v) where T : IComparable<T>
    {
        return IsSortedUsingComparator(v, (l, r) => l.CompareTo(r) < 0);
    }
}