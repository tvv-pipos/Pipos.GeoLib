using System.Collections;

namespace RoutingKit;

public static class ContractionHierarchyIO
{
    public static void WriteToFile(ContractionHierarchy ch, string filename)
    {
        using (var fileStream = new FileStream(filename, FileMode.Create))
        using (var writer = new BinaryWriter(fileStream))
        {
            // Header
            writer.Write(CHFileHeaderConstants.CHMagicNumber); //magic
            writer.Write(ch.Forward.FirstOut.Length - 1); // nodecount
            writer.Write(ch.Forward.Head.Count); // ForwardArcCount
            writer.Write(ch.Backward.Head.Count); // BackwardArcCount

            // ContractionHierarchy
            WriteVector(writer, ch.Rank);

            WriteVector(writer, ch.Forward.FirstOut);
            WriteVector(writer, ch.Forward.Head);
            WriteVector(writer, ch.Forward.Weight);
            WriteBitArray(writer, ch.Forward.IsShortcutAnOriginalArc);
            WriteVector(writer, ch.Forward.ShortcutFirstArc);
            WriteVector(writer, ch.Forward.ShortcutSecondArc);

            WriteVector(writer, ch.Backward.FirstOut);
            WriteVector(writer, ch.Backward.Head);
            WriteVector(writer, ch.Backward.Weight);
            WriteBitArray(writer, ch.Backward.IsShortcutAnOriginalArc);
            WriteVector(writer, ch.Backward.ShortcutFirstArc);
            WriteVector(writer, ch.Backward.ShortcutSecondArc);
        }
    }


    public static ContractionHierarchy ReadFile(string filename)
    {
        using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
        using (BinaryReader reader = new BinaryReader(stream))
        {
            var header = new CHFileHeader
            {
                MagicNumber = reader.ReadInt64(),
                NodeCount = reader.ReadInt32(),
                ForwardArcCount = reader.ReadInt32(),
                BackwardArcCount = reader.ReadInt32()
            };

            var ch = new ContractionHierarchy();
            ch.Rank = ReadArray<int>(reader, header.NodeCount);
            ch.Order = Permutation.InvertPermutation(ch.Rank);

            ch.Forward.FirstOut = ReadArray<int>(reader, header.NodeCount + 1);
            ch.Forward.Head = ReadVector(reader, header.ForwardArcCount);
            ch.Forward.Weight = ReadVector(reader, header.ForwardArcCount);
            ch.Forward.IsShortcutAnOriginalArc = ReadBitVector(reader, header.ForwardArcCount);
            ch.Forward.ShortcutFirstArc = ReadArray<int>(reader, header.ForwardArcCount);
            ch.Forward.ShortcutSecondArc = ReadArray<int>(reader, header.ForwardArcCount);

            ch.Backward.FirstOut = ReadArray<int>(reader, header.NodeCount + 1);
            ch.Backward.Head = ReadVector(reader, header.BackwardArcCount);
            ch.Backward.Weight = ReadVector(reader, header.BackwardArcCount);
            ch.Backward.IsShortcutAnOriginalArc = ReadBitVector(reader, header.BackwardArcCount);
            ch.Backward.ShortcutFirstArc = ReadArray<int>(reader, header.BackwardArcCount);
            ch.Backward.ShortcutSecondArc = ReadArray<int>(reader, header.BackwardArcCount);

            return ch;
        }
    }


    public static void WriteHeader(BinaryWriter writer, CHFileHeader header)
    {
        writer.Write(header.MagicNumber);
        writer.Write(header.NodeCount);
        writer.Write(header.ForwardArcCount);
        writer.Write(header.BackwardArcCount);
    }

    public static void WriteVector(BinaryWriter writer, IEnumerable<int> vector)
    {
        foreach (var item in vector)
        {
            writer.Write(item);
        }
    }

    public static void WriteBitArray(BinaryWriter writer, BitArray bitArray)
    {
        byte[] bytes = new byte[(bitArray.Length + 7) / 8];
        bitArray.CopyTo(bytes, 0);
        writer.Write(bytes);
    }

    public static List<int> ReadVector(BinaryReader reader, int count)
    {
        byte[] buffer = new byte[count * sizeof(int)];
        if (reader.Read(buffer, 0, buffer.Length) != buffer.Length)
            throw new InvalidOperationException("Failed to read from the stream.");
        List<int> result = new List<int>(count);
        for (int i = 0; i < buffer.Length; i += sizeof(int))
        {
            result.Add(BitConverter.ToInt32(buffer, i));
        }
        return result;
    }

    public static int[] ReadArray<T>(BinaryReader reader, int count)
    {
        byte[] buffer = new byte[count * sizeof(int)];
        if (reader.Read(buffer, 0, buffer.Length) != buffer.Length)
            throw new InvalidOperationException("Failed to read from the stream.");
        var result = new int[count];
        var idx = 0;
        for (int i = 0; i < buffer.Length; i += sizeof(int))
        {
            result[idx++] = BitConverter.ToInt32(buffer, i);
        }
        return result;
    }

    static BitArray ReadBitVector(BinaryReader reader, int count)
    {
        byte[] buffer = new byte[(count + 7) / 8];
        if (reader.Read(buffer, 0, buffer.Length) != buffer.Length)
            throw new InvalidOperationException("Failed to read from the stream.");
        BitArray result = new BitArray(buffer);
        result.Length = count;
        return result;
    }
}