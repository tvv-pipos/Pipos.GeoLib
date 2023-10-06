public static class Detail
{

    // public static List<int> ComputeMaybeStableSortPermutationUsingKey<T, K>(List<T> v, int keyCount, Func<T, int> getKey)
    // {
    //     List<int> p = new List<int>();

    //     if (v.Count >= keyCount / bucketSortMinKeyToElementRatio)
    //     {
    //         p.Capacity = v.Count;
    //         List<int> keyPos = ComputeKeyPos(v, keyCount, getKey);
    //         for (int i = 0; i < v.Count; ++i)
    //         {
    //             int k = getKey(v[i]);
    //             if (k > keyCount)
    //             {
    //                 throw new ArgumentException("Key is too large");
    //             }
    //             p.Insert(keyPos[k], i);
    //             keyPos[k]++;
    //         }
    //     }
    //     else if (isStable)
    //     {
    //         p = ComputeStableSortPermutationUsingComparator(v, MakeCompareByKey(keyCount, getKey));
    //     }
    //     else
    //     {
    //         p = ComputeSortPermutationUsingComparator(v, MakeCompareByKey(keyCount, getKey));
    //     }
    //     return p;
    // }

    // private const int bucketSortMinKeyToElementRatio = 16;

    // private static List<int> ComputeKeyPos<T, K>(List<T> v, int keyCount, Func<T, int> getKey)
    // {
    //     List<int> keyPos = new List<int>(keyCount);
    //     for (int i = 0; i < keyCount; ++i)
    //     {
    //         keyPos.Add(0);
    //     }

    //     foreach (T item in v)
    //     {
    //         int k = getKey(item);
    //         if (k > keyCount)
    //         {
    //             throw new ArgumentException("Key is too large");
    //         }
    //         keyPos[k]++;
    //     }

    //     int sum = 0;
    //     for (int i = 0; i < keyCount; ++i)
    //     {
    //         int tmp = sum + keyPos[i];
    //         keyPos[i] = sum;
    //         sum = tmp;
    //     }

    //     return keyPos;
    // }

    // private static List<int> ComputeStableSortPermutationUsingComparator<T, K>(List<T> v, CompareByKey<K> comparer)
    // {
    //     List<int> p = new List<int>(v.Count);
    //     for (int i = 0; i < v.Count; ++i)
    //     {
    //         p.Add(i);
    //     }
    //     p.Sort(comparer);
    //     return p;
    // }

    // private static List<int> ComputeSortPermutationUsingComparator<T, K>(List<T> v, CompareByKey<K> comparer)
    // {
    //     List<int> p = new List<int>(v.Count);
    //     for (int i = 0; i < v.Count; ++i)
    //     {
    //         p.Add(i);
    //     }
    //     p.Sort(comparer);
    //     return p;
    // }

    // private static CompareByKey<K> MakeCompareByKey<T, K>(int n, Func<T, int> k)
    // {
    //     return new CompareByKey<K>(n, k);
    // }

    // private static bool isStable = true;

    // public class CompareByKey<K>
    // {
    //     private readonly Func<int, int, bool> compare;
    //     private readonly Func<K, int> getKey;

    //     public CompareByKey(int n, Func<K, int> k)
    //     {
    //         compare = (l, r) =>
    //         {
    //             int lKey = getKey((K)l);
    //             if (lKey >= n)
    //             {
    //                 throw new ArgumentException("Key is too large");
    //             }

    //             int rKey = getKey((K)r);
    //             if (rKey >= n)
    //             {
    //                 throw new ArgumentException("Key is too large");
    //             }

    //             return lKey < rKey;
    //         };

    //         getKey = k;
    //     }

    //     public int Compare(int l, int r)
    //     {
    //         return compare(l, r) ? -1 : 1;
    //     }
    // }
}