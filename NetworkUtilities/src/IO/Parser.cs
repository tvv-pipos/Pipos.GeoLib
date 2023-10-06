namespace Pipos.Common.NetworkUtilities.IO;

public static class Parser
{
    public static int[][] ParseLineString(string wkt)
    {
        //LINESTRING Z (516549.868 6491440.437 -99999,516552.832 6491437.796 -99999,516572.515 6491422.038 -99999)
        var first = wkt.IndexOf('(') + 1;
        var groups = wkt.Substring(first, wkt.Length - first - 1).Split(',');
        var coordinates = new int[groups.Length][];
        for (var i = 0; i < groups.Length; i++)
        {
            var coords = groups[i].Split(' ');
            coordinates[i] = new int[] {
                (int)Math.Round(ParseDouble(coords[0])),
                (int)Math.Round(ParseDouble(coords[1]))};
        }

        return coordinates;
    }
    public static List<Node> ParseNodes(int[][] lineString, int forwardSpeed, int backwardSpeed, 
        int networkGroup, int functionClass)
    {
        var nodes = new List<Node>();
        if (lineString == null || lineString.Length < 2)
        {
            return nodes;
        }

        for (var i = 0; i < lineString.Length; i++)
        {
            if (i == 0)
            {
                nodes.Add(new Node(lineString[0], networkGroup, functionClass));
                continue;
            }

            var source = nodes.Last();
            var target = new Node(lineString[i], networkGroup, functionClass);
            if (source.Id == target.Id)
            {
                continue;
            }
            var distance = source.DistanceTo(target);
            var timeForward = forwardSpeed == 0 ? 0 : (int)Math.Round((distance / (forwardSpeed / 3.6)) * 1000);
            var timeBackward = backwardSpeed == 0 ? 0 : (int)Math.Round((distance / (backwardSpeed / 3.6)) * 1000);
            var edge = new Edge(source, target, distance, timeForward, timeBackward);
            source.Edges.Add(edge);
            target.Edges.Add(edge);
            nodes.Add(target);
        }

        return nodes;
    }

    public static int ParseInt(string str)
    {
        var y = 0;
        for (var i = 0; i < str.Length; i++)
        {
            y = y * 10 + (str[i] - '0');
        }
        return y;
    }

    public static long ParseLong(string str)
    {
        var y = 0L;
        for (var i = 0; i < str.Length; i++)
        {
            y = y * 10L + (str[i] - '0');
        }
        return y;
    }

    public static double ParseDouble(string input)
    {
        var n = 0L;
        var decimalPosition = input.Length;
        char c;
        for (var k = 0; k < input.Length; k++)
        {
            c = input[k];
            if (c == '.')
            {
                decimalPosition = k + 1;
            }
            else
            {
                n = (n * 10) + (int)(c - '0');
            }
        }

        return Decimal.ToDouble(new decimal((int)n, (int)(n >> 32), 0, false, (byte)(input.Length - decimalPosition)));
    }
}