namespace Pipos.GeoLib.Core.Api;

public class ConnectionRule : IConnectionRule
{
    public bool NoFerry { get; set; }
    public bool NoMotorway { get; set; }
    public bool NoOneWay { get; set; }
    public bool NoDisconnectedIsland { get; set; }
    public bool OnlyDisconnectedIsland { get; set; }
}