using System.Text.Json.Serialization;

namespace Pipos.GeoLib.NetworkUtilities.Model;

public class Scenario
{
    [JsonPropertyName("nvdb")]
    public int NVDB { get; set; }

    [JsonPropertyName("fuel_price")]
    public int FuelPrice { get; set; }

    [JsonPropertyName("gtfs")]
    public int GTFS { get; set; }

    [JsonPropertyName("public_transport_fare")]
    public int PublicTransportFare { get; set; }

    [JsonPropertyName("car_availability")]
    public int CarAvailability { get; set; }

    [JsonPropertyName("activity_tile")]
    public int ActivityTile { get; set; }

    public static Scenario Empty = new Scenario(0);
    public Scenario() {}
    public Scenario(int scenario_id)
    {
        NVDB = scenario_id;
        FuelPrice = scenario_id;
        GTFS = scenario_id;
        PublicTransportFare = scenario_id;
        CarAvailability = scenario_id;
        ActivityTile = scenario_id;
    }

    public static implicit operator Scenario(int scenario_id)
    {   
        return new Scenario(scenario_id);
    }

    public override string ToString()
    {
        return $"{NVDB}_{FuelPrice}_{GTFS}_{PublicTransportFare}_{CarAvailability}_{ActivityTile}";
    }

    public UInt64 GetUniqueId()
    {
        return ((UInt64)(NVDB - 2000) & 0xFF) | 
               ((UInt64)((FuelPrice - 2000) & 0xFF) << 8) |
               ((UInt64)((GTFS - 2000) & 0xFF) << 16) |
               ((UInt64)((PublicTransportFare - 2000) & 0xFF) << 24) |
               ((UInt64)((CarAvailability - 2000) & 0xFF) << 32) |
               ((UInt64)((ActivityTile - 2000) & 0xFF) << 40);
    }
}