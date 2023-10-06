namespace RoutingKit;

public static class VectorIO
{
    public static List<int> LoadVectorInt32(string filePath)
    {
        var bytes = File.ReadAllBytes(filePath);
        var numBytesPerInt = sizeof(int);
        var numInts = bytes.Length / numBytesPerInt;
        var intList = new List<int>(numInts);

        for (var i = 0; i < numInts; i++)
        {
            var intValue = BitConverter.ToInt32(bytes, i * numBytesPerInt);
            intList.Add(intValue);
        }

        return intList;
    }

    public static List<long> LoadVectorInt64(string filePath)
    {
        var bytes = File.ReadAllBytes(filePath);
        var numBytesPerInt = sizeof(long);
        var numInts = bytes.Length / numBytesPerInt;
        var intList = new List<long>(numInts);

        for (var i = 0; i < numInts; i++)
        {
            var intValue = BitConverter.ToInt64(bytes, i * numBytesPerInt);
            intList.Add(intValue);
        }

        return intList;
    }

    public static void SaveVectorInt32(string filePath, List<int> intList)
    {
        var numInts = intList.Count;
        var bytes = new byte[numInts * sizeof(int)];

        for (var i = 0; i < numInts; i++)
        {
            var intBytes = BitConverter.GetBytes(intList[i]);
            Array.Copy(intBytes, 0, bytes, i * sizeof(int), sizeof(int));
        }

        File.WriteAllBytes(filePath, bytes);
    }

    public static void SaveVectorInt64(string filePath, List<long> longList)
    {
        var numLongs = longList.Count;
        var bytes = new byte[numLongs * sizeof(long)];

        for (var i = 0; i < numLongs; i++)
        {
            var longBytes = BitConverter.GetBytes(longList[i]);
            Array.Copy(longBytes, 0, bytes, i * sizeof(long), sizeof(long));
        }

        File.WriteAllBytes(filePath, bytes);
    }
}
