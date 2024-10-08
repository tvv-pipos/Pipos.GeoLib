using System.Collections;
using Pipos.GeoLib.Core.Api;

namespace Pipos.GeoLib.Road;

public class Connection : IConnection, IEnumerable<ConnectionPoint>
{
    private List<ConnectionPoint> Connections;
    public Connection() 
    {
        Connections = new List<ConnectionPoint>();
    }

    public void Add(ConnectionPoint point)
    {
        Connections.Add(point);
    }

    public bool IsConnected()
    {
        foreach(var connection in Connections)
        {
            if(connection.IsConnected())
                return true;
        }
        return false;
    }

    public IEnumerator<ConnectionPoint> GetEnumerator()
    {
        return Connections.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Connections.GetEnumerator();
    }
}   