using ProjNet;
using ProjNet.CoordinateSystems;

namespace Pipos.Common.NetworkUtilities.Processing;

public class CoordinateTransformation
{
    readonly CoordinateSystemServices _coordinateSystemServices;
    public CoordinateTransformation()
    {
        _coordinateSystemServices = new CoordinateSystemServices(
            new Dictionary<int, string>
            {
                // Coordinate systems:
                [4326] = GeographicCoordinateSystem.WGS84.WKT,

                [3857] = ProjectedCoordinateSystem.WebMercator.WKT,

                [3006] =
                    @"
                        PROJCS[""SWEREF99 TM"",
                            GEOGCS[""SWEREF99"",
                                DATUM[""SWEREF99"",
                                    SPHEROID[""GRS 1980"",6378137,298.257222101],
                                    TOWGS84[0,0,0,0,0,0,0]],
                                PRIMEM[""Greenwich"",0,
                                    AUTHORITY[""EPSG"",""8901""]],
                                UNIT[""degree"",0.0174532925199433,
                                    AUTHORITY[""EPSG"",""9122""]],
                                AUTHORITY[""EPSG"",""4619""]],
                            PROJECTION[""Transverse_Mercator""],
                            PARAMETER[""latitude_of_origin"",0],
                            PARAMETER[""central_meridian"",15],
                            PARAMETER[""scale_factor"",0.9996],
                            PARAMETER[""false_easting"",500000],
                            PARAMETER[""false_northing"",0],
                            UNIT[""metre"",1,
                                AUTHORITY[""EPSG"",""9001""]],
                            AUTHORITY[""EPSG"",""3006""]]
                    "
            });    
    }

    public (int x, int y) Wgs84ToSwref99(double longitude, double latitude)
    {
        var transformation = _coordinateSystemServices.CreateTransformation(4326, 3006);
        transformation.MathTransform.Transform(ref longitude, ref latitude);
        return ((int)Math.Round(longitude), (int)Math.Round(latitude));
    }
}