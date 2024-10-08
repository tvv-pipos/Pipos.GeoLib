using System;
using Pipos.GeoLib.Core.Model;

namespace Pipos.GeoLib.Core.Api;

public interface ILoader
{
    public ILoader FromFile(string filename, YearSet years); 
    public ILoader FromGeoJunkJill(Uri uri, YearSet years);
    public INetworkManager BuildNetworkManager();
}
