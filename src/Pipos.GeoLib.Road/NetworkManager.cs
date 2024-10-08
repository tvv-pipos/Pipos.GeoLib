using Pipos.GeoLib.Core.Api;

namespace Pipos.GeoLib.Road;

public class NetworkManager : INetworkManager
{
    private Network Network;

    public NetworkManager(Network network)
    {
        Network = network;
    } 

    public INetwork LoadNetwork()
    {
        return Network;
    }
}