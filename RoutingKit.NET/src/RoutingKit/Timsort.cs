namespace RoutingKit;

public class Timsort
{
    // Minsta storlek för en delarray som ska använda insertion sort
    private const int MIN_MERGE = 32;

    // Sortera arrayen med timsort
    public static void Sort<T>(T[] array) where T : IComparable<T>
    {
        Sort(array, 0, array.Length);
    }

    public static void Sort<T>(T[] array, IComparer<T> comparer) where T : IComparable<T>
    {
        Sort(array, 0, array.Length, comparer);
    }

    // Sortera en del av arrayen med timsort
    public static void Sort<T>(T[] array, int left, int right, IComparer<T> comparer) where T : IComparable<T>
    {
        int len = right - left;
        if (len < 2)
            return;

        if (len < MIN_MERGE)
        {
            InsertionSort(array, left, right, comparer);
            return;
        }

        int minRun = CalculateMinRun(len);

        for (int i = left; i < right; i += minRun)
        {
            int end = Math.Min(i + minRun, right);
            InsertionSort(array, i, end, comparer);
        }

        while (minRun < len)
        {
            for (int i = left; i < right; i += minRun * 2)
            {
                int mid = Math.Min(i + minRun, right);
                int end = Math.Min(i + minRun * 2, right);
                Merge(array, i, mid, end, comparer);
            }
            minRun *= 2;
        }
    }

    public static void Sort<T>(T[] array, int left, int right) where T : IComparable<T>
    {
        int len = right - left;
        if (len < 2)
            return;

        if (len < MIN_MERGE)
        {
            InsertionSort(array, left, right);
            return;
        }

        int minRun = CalculateMinRun(len);

        for (int i = left; i < right; i += minRun)
        {
            int end = Math.Min(i + minRun, right);
            InsertionSort(array, i, end);
        }

        while (minRun < len)
        {
            for (int i = left; i < right; i += minRun * 2)
            {
                int mid = Math.Min(i + minRun, right);
                int end = Math.Min(i + minRun * 2, right);
                Merge(array, i, mid, end);
            }
            minRun *= 2;
        }
    }

    // Utför insertion sort på en del av arrayen
    private static void InsertionSort<T>(T[] array, int left, int right) where T : IComparable<T>
    {
        for (int i = left + 1; i < right; i++)
        {
            T key = array[i];
            int j = i - 1;
            while (j >= left && array[j].CompareTo(key) > 0)
            {
                array[j + 1] = array[j];
                j--;
            }
            array[j + 1] = key;
        }
    }

    private static void InsertionSort<T>(T[] array, int left, int right, IComparer<T> comparer) where T : IComparable<T>
    {
        for (int i = left + 1; i < right; i++)
        {
            T key = array[i];
            int j = i - 1;
            while (j >= left && comparer.Compare(array[j], key) > 0)
            {
                array[j + 1] = array[j];
                j--;
            }
            array[j + 1] = key;
        }
    }

    // Sammanfoga två delar av arrayen
    private static void Merge<T>(T[] array, int left, int mid, int right) where T : IComparable<T>
    {
        int len1 = mid - left;
        int len2 = right - mid;

        T[] leftArray = new T[len1];
        T[] rightArray = new T[len2];

        Array.Copy(array, left, leftArray, 0, len1);
        Array.Copy(array, mid, rightArray, 0, len2);

        int i = 0, j = 0, k = left;

        while (i < len1 && j < len2)
        {
            if (leftArray[i].CompareTo(rightArray[j]) <= 0)
            {
                array[k] = leftArray[i];
                i++;
            }
            else
            {
                array[k] = rightArray[j];
                j++;
            }
            k++;
        }

        while (i < len1)
        {
            array[k] = leftArray[i];
            i++;
            k++;
        }

        while (j < len2)
        {
            array[k] = rightArray[j];
            j++;
            k++;
        }
    }

    private static void Merge<T>(T[] array, int left, int mid, int right, IComparer<T> comparer) where T : IComparable<T>
    {
        int len1 = mid - left;
        int len2 = right - mid;

        T[] leftArray = new T[len1];
        T[] rightArray = new T[len2];

        Array.Copy(array, left, leftArray, 0, len1);
        Array.Copy(array, mid, rightArray, 0, len2);

        int i = 0, j = 0, k = left;

        while (i < len1 && j < len2)
        {
            if (comparer.Compare(leftArray[i], rightArray[j]) <= 0)
            {
                array[k] = leftArray[i];
                i++;
            }
            else
            {
                array[k] = rightArray[j];
                j++;
            }
            k++;
        }

        while (i < len1)
        {
            array[k] = leftArray[i];
            i++;
            k++;
        }

        while (j < len2)
        {
            array[k] = rightArray[j];
            j++;
            k++;
        }
    }

    // Beräkna den minsta delarraystorleken som ska använda insertion sort
    private static int CalculateMinRun(int n)
    {
        int r = 0;
        while (n >= MIN_MERGE)
        {
            r |= (n & 1);
            n >>= 1;
        }
        return n + r;
    }
}