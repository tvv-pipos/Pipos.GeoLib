using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;

namespace RoutingKit;

public struct IDKeyPair
{
    // public IDKeyPair() {}

    // public IDKeyPair(int id, int key)
    // {
    //     Id = id;
    //     Key = key;
    // }
    public int Id;// { get; set; }
    public int Key;// { get; set; }
}

public class MinIDQueue
{
    private const int treeArity = 4;

    private int[] _idPos;
    private IDKeyPair[] _heap;
    private int _heapSize;

    public MinIDQueue(int idCount)
    {
        _idPos = new int[idCount];
        _heap = new IDKeyPair[idCount];
        Array.Fill(_idPos, Constants.INVALID_ID);
        _heapSize = 0;

        // for (var i = 0; i < idCount; i++)
        // {
        //     _idPos.Add(Constants.INVALID_ID);
        //     _heap.Add(new IDKeyPair());
        // }
    }

    public bool IsEmpty => _heapSize == 0;

    public int Size => _heapSize;

    public int IdCount => _idPos.Length;

    public bool ContainsId(int id)
    {
        if (id < IdCount)
        {
            return _idPos[id] != Constants.INVALID_ID;
        }
        return false;
    }

    public void Clear()
    {
        for (int i = 0; i < _heapSize; ++i)
        {
            _idPos[_heap[i].Id] = Constants.INVALID_ID;
        }
        _heapSize = 0;
    }

    public int GetKey(int id)
    {
        if (id < IdCount && _idPos[id] != Constants.INVALID_ID)
        {
            return _heap[_idPos[id]].Key;
        }
        throw new InvalidOperationException("Element is not part of the queue.");
    }

    public IDKeyPair Peek()
    {
        if (!IsEmpty)
        {
            return _heap[0];
        }
        throw new InvalidOperationException("Queue is empty.");
    }

    public IDKeyPair Pop()
    {
        if (!IsEmpty)
        {
            --_heapSize;
            Swap(ref _heap[0].Key, ref _heap[_heapSize].Key);
            Swap(ref _heap[0].Id, ref _heap[_heapSize].Id);

            _idPos[_heap[0].Id] = 0;
            _idPos[_heap[_heapSize].Id] = Constants.INVALID_ID;

            MoveDownInTree(0);
            return _heap[_heapSize];
        }
        throw new InvalidOperationException("Queue is empty.");
    }

    public void Push(int id, int key)
    {
        Debug.Assert(!ContainsId(id));

        int pos = _heapSize;
        ++_heapSize;
        _heap[pos].Id = id;
        _heap[pos].Key = key;
        _idPos[id] = pos;
        MoveUpInTree(pos);
    }

    public bool DecreaseKey(int id, int key)
    {
        if (id < IdCount && ContainsId(id))
        {
            int pos = _idPos[id];
            if (_heap[pos].Key > key)
            {
                _heap[pos].Key = key;
                MoveUpInTree(pos);
                return true;
            }
            return false;
        }
        throw new InvalidOperationException("Element is not part of the queue.");
    }

    public bool IncreaseKey(int id, int key)
    {
        if (id < IdCount && ContainsId(id))
        {
            int pos = _idPos[id];
            if (_heap[pos].Key < key)
            {
                _heap[pos].Key = key;
                MoveDownInTree(pos);
                return true;
            }
            return false;
        }
        throw new InvalidOperationException("Element is not part of the queue.");
    }

    private static void Swap<T>(ref T item1, ref T item2)
    {
        T tmp = item1;
        item1 = item2;
        item2 = tmp;
    }

    private void MoveUpInTree(int pos)
    {
        while (pos != 0)
        {
            int parent = (pos - 1) / treeArity;
            if (_heap[parent].Key > _heap[pos].Key)
            {
                Swap(ref _heap[pos],  ref _heap[parent]);
				Swap(ref _idPos[_heap[pos].Id], ref _idPos[_heap[parent].Id]);
            }
            pos = parent;
        }
    }

    private void MoveDownInTree(int pos)
    {
        while (true)
        {
            int firstChild = treeArity * pos + 1;
            if (firstChild >= _heapSize)
            {
                return; // no children
            }
            int smallestChild = firstChild;
            for (int c = firstChild + 1; c < Math.Min(treeArity * pos + treeArity + 1, _heapSize); ++c)
            {
                if (_heap[smallestChild].Key > _heap[c].Key)
                {
                    smallestChild = c;
                }
            }

            if (_heap[smallestChild].Key >= _heap[pos].Key)
            {
                return; // no child is smaller
            }

            Swap(ref _heap[pos], ref _heap[smallestChild]);
			Swap(ref _idPos[_heap[pos].Id], ref _idPos[_heap[smallestChild].Id]);
            pos = smallestChild;
        }
    }
}
