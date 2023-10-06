namespace Utils;

using System;
using System.Collections.Generic;

// Static 2d spatial index implemented using packed Hilbert R-tree.
public class KDIndex
{
    bool _finished;
    int[] _ids;
    int[] _coords;
    int _nodeSize;
    int _pos;
    int _numItems;

    // properties for testing purpose
    public int[] Ids => _ids;
    public int[] Coords => _coords;

    public KDIndex(int numItems, int nodeSize = 64) 
    {
        if (numItems < 0)
        {
            throw new Exception($"Unexpected numItems value: ${numItems}.");
        } 

        _numItems = numItems;
        _nodeSize = Math.Min(Math.Max(nodeSize, 2), 65535);
        _ids = new int[numItems];
        _coords = new int[numItems * 2];
        _pos = 0;
        _finished = false;
    }

    public int Add(int x, int y)
    {
        var index = _pos >> 1;
        _ids[index] = index;
        _coords[_pos++] = x;
        _coords[_pos++] = y;
        return index;
    }

    public KDIndex Finish() 
    {
        var numAdded = _pos >> 1;
        if (numAdded != _numItems) 
        {
            throw new Exception($"Added ${numAdded} items when expected ${_numItems}.");
        }
        // kd-sort both arrays for efficient search
        Sort(_ids, _coords, _nodeSize, 0, _numItems - 1, 0);

        _finished = true;
        return this;
    }

    public List<int> Range(int minX, int minY, int maxX, int maxY) {
        if (!_finished) 
        {
            throw new Exception("Data not yet indexed - call index.finish().");
        }

        var stack = new Stack<int>(new int[] { 0, _ids.Length - 1, 0 });
        var result = new List<int>();
        int x;
        int y; 

        // recursively search for items in range in the kd-sorted arrays
        while (stack.Any()) 
        {
            var axis = stack.Pop();
            var right = stack.Pop();
            var left = stack.Pop();

            // if we reached "tree node", search linearly
            if (right - left <= _nodeSize) 
            {
                for (var i = left; i <= right; i++) 
                {
                    x = _coords[2 * i];
                    y = _coords[2 * i + 1];
                    if (x >= minX && x <= maxX && y >= minY && y <= maxY) 
                    {
                        result.Add(_ids[i]);
                    }
                }
                continue;
            }

            // otherwise find the middle index
            var m = (left + right) >> 1;

            // include the middle item if it's in range
            x = _coords[2 * m];
            y = _coords[2 * m + 1];
            if (x >= minX && x <= maxX && y >= minY && y <= maxY) 
            {
                result.Add(_ids[m]);
            }

            // queue search in halves that intersect the query
            if (axis == 0 ? minX <= x : minY <= y) 
            {
                stack.Push(left);
                stack.Push(m - 1);
                stack.Push(1 - axis);
            }
            if (axis == 0 ? maxX >= x : maxY >= y) 
            {
                stack.Push(m + 1);
                stack.Push(right);
                stack.Push(1 - axis);
            }
        }

        return result;
    }

    public List<int> Within(int qx, int qy, int r)
    {
        if (!_finished) 
        {
            throw new Exception("Data not yet indexed - call index.finish().");
        }

        var stack = new Stack<int>(new int[] { 0, _ids.Length - 1, 0 });
        var result = new List<int>();
        var r2 = r * r;

        // recursively search for items within radius in the kd-sorted arrays
        while (stack.Any())
        {
            var axis = stack.Pop();
            var right = stack.Pop();
            var left = stack.Pop();

            // if we reached "tree node", search linearly
            if (right - left <= _nodeSize)
            {
                for (var i = left; i <= right; i++)
                {
                    if (SqDist(_coords[2 * i], _coords[2 * i + 1], qx, qy) <= r2) 
                    {
                        result.Add(_ids[i]);
                    }
                }
                continue;
            }

            // otherwise find the middle index
            var m = (left + right) >> 1;

            // include the middle item if it's in range
            var x = _coords[2 * m];
            var y = _coords[2 * m + 1];
            if (SqDist(x, y, qx, qy) <= r2) 
            {
                result.Add(_ids[m]);
            }

            // queue search in halves that intersect the query
            if (axis == 0 ? qx - r <= x : qy - r <= y)
            {
                stack.Push(left);
                stack.Push(m - 1);
                stack.Push(1 - axis);
            }
            if (axis == 0 ? qx + r >= x : qy + r >= y)
            {
                stack.Push(m + 1);
                stack.Push(right);
                stack.Push(1 - axis);
            }
        }

        return result;
    }


    static void Sort(int[] ids, int[] coords, int nodeSize, int left, int right, int axis)
    {
        if (right - left <= nodeSize)
        {
            return;
        }

        var m = (left + right) >> 1; // middle index

        // sort ids and coords around the middle index so that the halves lie
        // either left/right or top/bottom correspondingly (taking turns)
        Select(ids, coords, m, left, right, axis);

        // recursively kd-sort first half and second half on the opposite axis
        Sort(ids, coords, nodeSize, left, m - 1, 1 - axis);
        Sort(ids, coords, nodeSize, m + 1, right, 1 - axis);
    }

    static void Select(int[] ids, int[] coords, int k, int left, int right, int axis)
    {

        while (right > left)
        {
            if (right - left > 600)
            {
                var n = right - left + 1;
                var m = k - left + 1;
                var z = Math.Log(n);
                var s = 0.5 * Math.Exp(2 * z / 3);
                var sd = 0.5 * Math.Sqrt(z * s * (n - s) / n) * (m - n / 2 < 0 ? -1 : 1);
                var newLeft = (int)Math.Max(left, Math.Floor(k - m * s / n + sd));
                var newRight = (int)Math.Min(right, Math.Floor(k + (n - m) * s / n + sd));
                Select(ids, coords, k, newLeft, newRight, axis);
            }

            var t = coords[2 * k + axis];
            var i = left;
            var j = right;

            SwapItem(ids, coords, left, k);
            if (coords[2 * right + axis] > t)
            {
                SwapItem(ids, coords, left, right);
            }

            while (i < j)
            {
                SwapItem(ids, coords, i, j);
                i++;
                j--;
                while (coords[2 * i + axis] < t) i++;
                while (coords[2 * j + axis] > t) j--;
            }

            if (coords[2 * left + axis] == t)
            {
                SwapItem(ids, coords, left, j);
            }
            else
            {
                j++;
                SwapItem(ids, coords, j, right);
            }

            if (j <= k) left = j + 1;
            if (k <= j) right = j - 1;
        }
    }

    static void SwapItem(int[] ids, int[] coords, int i, int j)
    {
        Swap(ids, i, j);
        Swap(coords, 2 * i, 2 * j);
        Swap(coords, 2 * i + 1, 2 * j + 1);
    }

    static void Swap(int[] arr, int i, int j)
    {
        var tmp = arr[i];
        arr[i] = arr[j];
        arr[j] = tmp;
    }

    static long SqDist(int ax, int ay, int bx, int by)
    {
        long dx = ax - bx;
        long dy = ay - by;
        return dx * dx + dy * dy;
    }
}
