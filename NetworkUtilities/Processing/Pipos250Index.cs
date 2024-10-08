﻿using System.Diagnostics;
using Pipos.GeoLib.NetworkUtilities.Model;

namespace Pipos.GeoLib.NetworkUtilities.Processing;

public class Pipos250Index
{
    private struct Coord { public int id;  public int x; public int y; }

    private int _Next = 0;
    private List<Coord>[] _Grid;

    public Pipos250Index()
	{
        int size = Grid250.Width * Grid250.Height; 
        _Grid = new List<Coord>[size];
    }

    public bool InRange(int piposId)
    {
        int x = PiposID.XFromId(piposId) + 125;
        int y = PiposID.YFromId(piposId) + 125;
        return Sweden.XMin < x && x < Sweden.XMax && Sweden.YMin < y && y < Sweden.YMax;
    }

    public void Add(int piposId)
    {
        int idx = Grid250.FromId(piposId);
        int x = PiposID.XFromId(piposId) + 125;
        int y = PiposID.YFromId(piposId) + 125;

        if (_Grid[idx] == null)
            _Grid[idx] = new List<Coord>();
        _Grid[idx].Add(new Coord { id = _Next++, x = x, y = y });
    }

    public void Add(int x , int y)
    {
        int idx = Grid250.FromCoordinate(x, y);
        Debug.Assert(idx >= 0 && idx < _Grid.Length);
        if (_Grid[idx] == null)
            _Grid[idx] = new List<Coord>();
        _Grid[idx].Add(new Coord { id = _Next++, x = x, y = y });
    }

    public int FindNearest(int piposId)
    {
        int idx = Grid250.FromId(piposId);
        int x = PiposID.XFromId(piposId);
        int y = PiposID.YFromId(piposId);
        return FindNearest(idx, x, y);
    }

    public int FindNearest(int x, int y)
    {
        int idx = Grid250.FromCoordinate(x, y);
        return FindNearest(idx, x, y);
    }

    public int FindNearest(int idx, int x, int y)
    {
        int n = 0;
        int id = Int32.MaxValue;
        double distance = Double.MaxValue;

        if (_Grid[idx] != null)
        {
            foreach (Coord coord in _Grid[idx])
            {
                double new_distance = Math.Sqrt((double)(coord.x - x) * (double)(coord.x - x) + (double)(coord.y - y) * (double)(coord.y - y));
                if (new_distance < distance || (new_distance == distance && id > coord.id))
                {
                    distance = new_distance;
                    id = coord.id;
                }
            }
        }

        int x1 = Grid250.XFromCoordiante(x) - 1;
        int y1 = Grid250.YFromCoordiante(y) - 1;
        int x2 = Grid250.XFromCoordiante(x) + 1;
        int y2 = Grid250.YFromCoordiante(y) + 1;

        while (n <= 1 && (x1 >= 0 || x2 < Grid250.Width || y1 >= 0 || y2 < Grid250.Height))
        {
            if(y1 >= 0)
            {
                // Calc lower row, clamp start end x values
                int start_x = x1 < 0 ? 0 : x1;
                int end_x = x2 >= Grid250.Width ? Grid250.Width : x2;

                for(int sx = start_x; sx < end_x; sx++)
                {
                    idx = sx + y1 * Grid250.Width;
                    if (_Grid[idx] != null)
                    {
                        foreach (Coord coord in _Grid[idx])
                        {
                            double new_distance = Math.Sqrt((double)(coord.x - x) * (double)(coord.x - x) + (double)(coord.y - y) * (double)(coord.y - y));
                            if (new_distance < distance || (new_distance == distance && id > coord.id))
                            {
                                distance = new_distance;
                                id = coord.id;
                            }
                        }
                    }
                }
            }

            if(x2 < Grid250.Width)
            {
                // Calc lower row, clamp start end y values
                int start_y = y1 < 0 ? 0 : y1;
                int end_y = y2 >= Grid250.Height ? Grid250.Height : y2;

                for (int sy = start_y; sy < end_y; sy++)
                {
                    idx = x2 + sy * Grid250.Width;
                    if (_Grid[idx] != null)
                    {
                        foreach (Coord coord in _Grid[idx])
                        {
                            double new_distance = Math.Sqrt((double)(coord.x - x) * (double)(coord.x - x) + (double)(coord.y - y) * (double)(coord.y - y));
                            if (new_distance < distance || (new_distance == distance && id > coord.id))
                            {
                                distance = new_distance;
                                id = coord.id;
                            }
                        }
                    }
                }
            }

            if(y2 < Grid250.Height)
            {
                // Calc lower row, clamp start end x values
                int start_x = x1 < 0 ? 0 : x1;
                int end_x = x2 >= Grid250.Width ? Grid250.Width : x2;

                for (int sx = start_x; sx < end_x; sx++)
                {
                    idx = sx + y2 * Grid250.Width;
                    if (_Grid[idx] != null)
                    {
                        foreach (Coord coord in _Grid[idx])
                        {
                            double new_distance = Math.Sqrt((double)(coord.x - x) * (double)(coord.x - x) + (double)(coord.y - y) * (double)(coord.y - y));
                            if (new_distance < distance || (new_distance == distance && id > coord.id))
                            {
                                distance = new_distance;
                                id = coord.id;
                            }
                        }
                    }
                }
            }

            if(x1 >= 0)
            {
                // Calc lower row, clamp start end y values
                int start_y = y1 < 0 ? 0 : y1;
                int end_y = y2 >= Grid250.Height ? Grid250.Height : y2;

                for (int sy = start_y; sy < end_y; sy++)
                {
                    idx = x1 + sy * Grid250.Width;
                    if (_Grid[idx] != null)
                    {
                        foreach (Coord coord in _Grid[idx])
                        {
                            double new_distance = Math.Sqrt((double)(coord.x - x) * (double)(coord.x - x) + (double)(coord.y - y) * (double)(coord.y - y));
                            if (new_distance < distance || (new_distance == distance && id > coord.id))
                            {
                                distance = new_distance;
                                id = coord.id;
                            }
                        }
                    }
                }
            }

            if (id != Int32.MaxValue)
                n++;

            x1 = x1 - 1;
            y1 = y1 - 1;
            x2 = x2 + 1;
            y2 = y2 + 1;
        }
        return id;
    }

}
