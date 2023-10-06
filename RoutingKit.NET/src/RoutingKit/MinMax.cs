namespace RoutingKit;

public static class MinMax
{
    public static int FirstMaxElementPositionOf<T>(List<T> v) where T : IComparable<T>
    {
        if (v.Count == 0)
        {
            throw new InvalidOperationException("List is empty.");
        }

        int pos = 0;
        for (int i = 1; i < v.Count; ++i)
        {
            if (v[i].CompareTo(v[pos]) > 0)
            {
                pos = i;
            }
        }
        return pos;
    }

    public static int FirstMaxElementPositionOf<T>(T[] v) where T : IComparable<T>
    {
        if (v.Length == 0)
        {
            throw new InvalidOperationException("List is empty.");
        }

        int pos = 0;
        for (int i = 1; i < v.Length; ++i)
        {
            if (v[i].CompareTo(v[pos]) > 0)
            {
                pos = i;
            }
        }
        return pos;
    }

    public static T MaxElementOf<T>(List<T> v) where T : IComparable<T>
    {
        int pos = FirstMaxElementPositionOf(v);
        return v[pos];
    }

    public static T MaxElementOf<T>(T[] v) where T : IComparable<T>
    {
        int pos = FirstMaxElementPositionOf(v);
        return v[pos];
    }

    public static T MaxElementOf<T>(List<T> v, T emptyValue) where T : IComparable<T>
    {
        if (v.Count == 0)
        {
            return emptyValue;
        }
        else
        {
            int pos = FirstMaxElementPositionOf(v);
            return v[pos];
        }
    }
}