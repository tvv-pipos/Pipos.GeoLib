using System.Diagnostics;
using System.IO.Compression;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Pipos.GeoLib.NetworkUtilities.Model;
using Pipos.GeoLib.NetworkUtilities.Processing;

namespace Pipos.GeoLib.NetworkUtilities.IO;

public class GTFSLoader
{   
    readonly ILogger<GTFSLoader> _logger;
    readonly static CoordinateTransformation _transformer = new CoordinateTransformation();
    readonly static Regex _csvParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
    public GTFSLoader(ILogger<GTFSLoader> logger)
    {
        _logger = logger;
    }

    public GTFSData ReadData(string zipFile)
    {
        var sw = Stopwatch.StartNew();
        var stops = new List<GTFSStop>();
        var stopTimes = new List<GTFSStopTime>();
        var transfers = new List<GTFSTransfer>();
        var tmpDir = Directory.CreateTempSubdirectory();

        using (var zip = ZipFile.OpenRead(zipFile))
        {
            zip.ExtractToDirectory(tmpDir.FullName);

            _logger.LogDebug("Memory used before allocation:       {0:N0}",
            GC.GetTotalMemory(false));

            foreach (var filePath in Directory.EnumerateFiles(tmpDir.FullName))
            {
                var filename = Path.GetFileName(filePath);
                if (filename == "stops.txt")
                {
                    using (var streamReader = new StreamReader(filePath))
                    {
                        var line = streamReader.ReadLine();
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            stops.Add(ParseStop(line));
                        }
                    }
                }

                if (filename == "stop_times.txt")
                {
                    using (var streamReader = new StreamReader(filePath))
                    {
                        var line = streamReader.ReadLine();
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            stopTimes.Add(ParseStopTime(line));
                        }
                    }
                }

                if (filename == "transfers.txt")
                {
                    using (var streamReader = new StreamReader(filePath))
                    {
                        var line = streamReader.ReadLine();
                        while ((line = streamReader.ReadLine()) != null)
                        {
                            transfers.Add(ParseTransfer(line));
                        }
                    }
                }
            }
        }
        
        _logger.LogDebug("Memory used before collection:       {0:N0}", GC.GetTotalMemory(false));
        GC.Collect();
        GC.WaitForPendingFinalizers();
        _logger.LogDebug("Memory used after full collection:   {0:N0}", GC.GetTotalMemory(true));

        var stopTimesPerStop = stopTimes
            .GroupBy(x => x.StopId)
            .ToDictionary(x => x.Key, v => v.OrderBy(i => i.DepartureTime)
            .ToList());

        var stopTimesPerTrip = stopTimes
            .OrderBy(x => x.StopSequence)
            .GroupBy(x => x.TripId)
            .ToDictionary(x => x.Key, v => v.ToList());

        var transfersFromStop = transfers
            .GroupBy(x => x.FromStopId)
            .ToDictionary(x => x.Key, v => v.ToList());

        var stopsDict = stops
            .ToDictionary(x => x.Id);

        _logger.LogDebug($"GTFS ReadData Elapsed {sw.Elapsed}");

        return new GTFSData(stopTimesPerStop, stopTimesPerTrip, transfersFromStop, stopsDict);
    }

    static GTFSStop ParseStop(string row)
    {
        var cols = _csvParser.Split(row);
        var (x, y) = _transformer.Wgs84ToSwref99(Parser.ParseDouble(cols[3]), Parser.ParseDouble(cols[2]));
        return new GTFSStop
        {
            Id = Parser.ParseLong(cols[0]),
            Name = cols[1],
            X = x,
            Y = y
        };
    }

    static int ParseDate(string date)
    {
        var items = date.Split(":");
        return Parser.ParseInt(items[0]) * 3600 +
            Parser.ParseInt(items[1]) * 60 +
            Parser.ParseInt(items[2]);
    }

    static GTFSStopTime ParseStopTime(string row)
    {
        var cols = row.Split(",");
        return new GTFSStopTime
        {
            TripId = Parser.ParseLong(cols[0]),
            ArrivalTime = ParseDate(cols[1]),
            DepartureTime = ParseDate(cols[2]),
            StopId = Parser.ParseLong(cols[3]),
            StopSequence = Parser.ParseInt(cols[4])
        };
    }

    static GTFSTransfer ParseTransfer(string row)
    {
        var cols = row.Split(",");
        return new GTFSTransfer
        {
            FromStopId = Parser.ParseLong(cols[0]),
            ToStopId = Parser.ParseLong(cols[1]),
            MinimumTransferTime = Parser.ParseInt(cols[3])
        };
    }
}