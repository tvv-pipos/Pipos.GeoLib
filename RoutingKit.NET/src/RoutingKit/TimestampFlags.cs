namespace RoutingKit;

public class TimestampFlags
{
    private uint[] lastSeen;
    private uint currentTimestamp;

    public TimestampFlags(int idCount)
    {
        lastSeen = new uint[idCount];
        currentTimestamp = 1;
    }

    public bool IsSet(int id)
    {
        return lastSeen[id] == currentTimestamp;
    }

    public void Set(int id)
    {
        lastSeen[id] = currentTimestamp;
    }

    public void ResetOne(int id)
    {
        lastSeen[id] = (uint)(currentTimestamp - 1);
    }

    public void ResetAll()
    {
        currentTimestamp++;
        if (currentTimestamp == 0)
        {
            Array.Fill<uint>(lastSeen, 0);
            currentTimestamp = 1;
        }
    }
}
