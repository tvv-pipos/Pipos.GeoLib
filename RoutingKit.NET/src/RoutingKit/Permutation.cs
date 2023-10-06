using System.Diagnostics;

namespace RoutingKit;

public static class Permutation
{
    public static int[] ChainPermutationFirstLeftThenRight(int[] p, int[] q)
    {
        if (!IsPermutation(p))
        {
            throw new ArgumentException("p must be a permutation");
        }

        if (!IsPermutation(q))
        {
            throw new ArgumentException("q must be a permutation");
        }

        if (p.Length != q.Length)
        {
            throw new ArgumentException("p and q must permute the same number of objects");
        }

        var r  = new int[p.Length];
        for (int i = 0; i < r.Length; ++i)
        {
            r[i] = p[q[i]];
        }
        return r;
    }

    public static int[] ChainPermutationFirstRightThenLeft(int[] p, int[] q)
    {
        return ChainPermutationFirstLeftThenRight(q, p);
    }

    public static List<T> ApplyPermutation<T>(int[] p, List<T> v)
    {
        if (!IsPermutation(p))
        {
            throw new ArgumentException("p must be a permutation");
        }

        if (p.Length != v.Count)
        {
            throw new ArgumentException("permutation and vector must have the same size");
        }

        var r = new List<T>(v.Count);
        for (int i = 0; i < v.Count; ++i)
        {
            r.Add(v[p[i]]);
        }
        return r;
    }

    public static int[] ApplyPermutation<T>(int[] p, int[] v)
    {
        if (!IsPermutation(p))
        {
            throw new ArgumentException("p must be a permutation");
        }

        if (p.Length != v.Length)
        {
            throw new ArgumentException("permutation and vector must have the same size");
        }

        var r = new int[v.Length];
        for (int i = 0; i < v.Length; ++i)
        {
            r[i] = v[p[i]];
        }
        return r;
    }

    // public static List<T> ApplyPermutationInplace<T>(List<int> p, List<T> v)
    // {
    //     if (!IsPermutation(p))
    //     {
    //         throw new ArgumentException("p must be a permutation");
    //     }

    //     if (p.Count != v.Count)
    //     {
    //         throw new ArgumentException("permutation and vector must have the same size");
    //     }

    //     List<T> r = new List<T>(new T[v.Count]);
    //     for (int i = 0; i < v.Count; ++i)
    //     {
    //         r[p[i]] = v[i];
    //     }
    //     return r;
    // }

    public static List<T> ApplyInversePermutation<T>(int[] p, List<T> v)
    {
        if (!IsPermutation(p))
        {
            throw new ArgumentException("p must be a permutation");
        }

        if (p.Length != v.Count)
        {
            throw new ArgumentException("permutation and vector must have the same size");
        }

        List<T> r = new List<T>(new T[v.Count]);
        for (int i = 0; i < v.Count; ++i)
        {
            r[p[i]] = v[i];
        }
        return r;
    }

    // public static T[] ApplyInversePermutation<T>(int[] p, T[] v)
    // {
    //     if (!IsPermutation(p))
    //     {
    //         throw new ArgumentException("p must be a permutation");
    //     }

    //     if (p.Length != v.Length)
    //     {
    //         throw new ArgumentException("permutation and vector must have the same size");
    //     }

    //     var r = new T[v.Length];
    //     for (int i = 0; i < v.Length; ++i)
    //     {
    //         r[p[i]] = v[i];
    //     }
    //     return r;
    // }

    public static T[] ApplyInversePermutation<T>(int[] p, T[] v)
    {
        if (!IsPermutation(p))
        {
            throw new ArgumentException("p must be a permutation");
        }

        if (p.Length != v.Length)
        {
            throw new ArgumentException("permutation and vector must have the same size");
        }

        var r = new T[v.Length];
        for (int i = 0; i < v.Length; ++i)
        {
            r[p[i]] = v[i];
        }
        return r;
    }

    // static List<T> ApplyInversePermutationInplace<T>(List<int> p, List<T> v)
    // {
    //     if (!IsPermutation(p))
    //     {
    //         throw new ArgumentException("p must be a permutation");
    //     }

    //     if (p.Count != v.Count)
    //     {
    //         throw new ArgumentException("permutation and vector must have the same size");
    //     }

    //     List<T> r = new List<T>(v.Count);
    //     for (int i = 0; i < v.Count; ++i)
    //     {
    //         r.Add(v[p[i]]);
    //     }
    //     return r;
    // }

    public static void InplaceApplyPermutationToElementsOf(int[] p, List<int> v)
    {
        if (!IsPermutation(p))
        {
            throw new ArgumentException("p must be a permutation");
        }

        if (!v.All(x => x < p.Length))
        {
            throw new ArgumentException("v has an out of bounds element");
        }

        for (int i = 0; i < v.Count; ++i)
        {
            v[i] = p[v[i]];
        }
    }

    public static List<int> ApplyPermutationToElementsOf(int[] p, List<int> v)
    {
        if (!IsPermutation(p))
        {
            throw new ArgumentException("p must be a permutation");
        }

        if (!v.All(x => x < p.Length))
        {
            throw new ArgumentException("v has an out of bounds element");
        }

        List<int> r = new List<int>(v);
        InplaceApplyPermutationToElementsOf(p, r);
        return r;
    }

    public static void InplaceApplyPermutationToPossiblyInvalidElementsOf(int[] p, List<int> v)
    {
        if (!IsPermutation(p))
        {
            throw new ArgumentException("p must be a permutation");
        }

        if (!v.All(x => x < p.Length || x == Constants.INVALID_ID))
        {
            throw new ArgumentException("v has an out of bounds element");
        }

        for (int i = 0; i < v.Count; ++i)
        {
            if (v[i] != Constants.INVALID_ID)
            {
                v[i] = p[v[i]];
            }
        }
    }

    public static List<int> ApplyPermutationToPossiblyInvalidElementsOf(int[] p, List<int> v)
    {
        if (!IsPermutation(p))
        {
            throw new ArgumentException("p must be a permutation");
        }

        if (!v.All(x => x < p.Length || x == Constants.INVALID_ID))
        {
            throw new ArgumentException("v has an out of bounds element");
        }

        var r = new List<int>(v);
        InplaceApplyPermutationToPossiblyInvalidElementsOf(p, r);
        return r;
    }

    public static bool IsPermutation(List<int> p)
    {
        var found = new bool[p.Count];
        foreach (int x in p)
        {
            if (x >= p.Count)
            {
                return false;
            }
            if (found[x])
            {
                return false;
            }
            found[x] = true;
        }
        return true;
    }

    public static bool IsPermutation(int[] p)
    {
        var found = new bool[p.Length];
        foreach (int x in p)
        {
            if (x >= p.Length)
            {
                return false;
            }
            if (found[x])
            {
                return false;
            }
            found[x] = true;
        }
        return true;
    }

    public static int[] IdentityPermutation(int n)
    {
        var p = new int[n];
        for (int i = 0; i < n; ++i)
        {
            p[i] = i;
        }
        return p;
    }

    public static int[] RandomPermutation(int n, Random gen)
    {
        var r = IdentityPermutation(n);
        Shuffle(r, gen);
        return r;
    }

    public static void Shuffle<T>(T[] list, Random rng)
    {
        int n = list.Length;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static int[] InvertPermutation(int[] p)
    {
        Debug.Assert(IsPermutation(p), "p must be a permutation");
        var invP = new int[p.Length];

        for (int i = 0; i < p.Length; ++i)
        {
            invP[p[i]] = i;
        }

        return invP;
    }

    public static List<int> InvertPermutation(List<int> p)
    {
        if (!IsPermutation(p))
        {
            throw new ArgumentException("p must be a permutation");
        }

        List<int> invP = new List<int>(p.Count);
        for (int i = 0; i < p.Count; ++i)
        {
            invP.Add(0);
        }

        for (int i = 0; i < p.Count; ++i)
        {
            invP[p[i]] = i;
        }

        return invP;
    }
}