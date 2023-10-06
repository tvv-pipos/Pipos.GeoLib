using System.Diagnostics;

namespace RoutingKit;

public static class Timer
{
    public static long GetMicroTime()
    {
        long microseconds = Stopwatch.GetTimestamp() / (Stopwatch.Frequency / 1000000);
        return microseconds;
    }
}