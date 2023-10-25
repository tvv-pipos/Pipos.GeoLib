namespace Pipos.Common.NetworkUtilities.Processing;

public class MinHeap
{
    readonly List<ulong> array = new List<ulong>();
    public void Add(uint key, uint weight)
    {
        array.Add((((ulong)key) << 32) | ((ulong)weight));
        ulong tmp = 0;
        int pi = 0;
        int ci = array.Count - 1; // child index; start at end
        while (ci > 0)
        {
            pi = (ci - 1) / 2; // parent index
            if ((0xFFFFFFFF & array[ci]) >= (0xFFFFFFFF & array[pi])) break; // child item is larger than (or equal) parent so we're done
            tmp = array[ci]; 
            array[ci] = array[pi]; 
            array[pi] = tmp;
            ci = pi;
        }
    }
    public uint RemoveMin()
    {
        // assumes pq is not empty; up to calling code
        int li = array.Count - 1; // last index (before removal)
        uint frontItem = (uint)(array[0] >> 32);   // fetch the front
        array[0] = array[li];
        array.RemoveAt(li);
        --li; // last index (after removal)

        ulong tmp = 0;
        int pi = 0, ci = 0, rc = 0; // parent index. start at front of pq
        while (true)
        {
            ci = pi * 2 + 1; // left child index of parent
            if (ci > li) 
                break;  // no children so done
            rc = ci + 1;     // right child
            
            if (rc <= li && (0xFFFFFFFF & array[rc]) < (0xFFFFFFFF & array[ci])) // if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
                ci = rc;
            
            if ((0xFFFFFFFF & array[pi]) <= (0xFFFFFFFF & array[ci])) break; // parent is smaller than (or equal to) smallest child so done
            
            tmp = array[pi]; 
            array[pi] = array[ci]; 
            array[ci] = tmp; // swap parent and child
            pi = ci;
        }
        return frontItem;
    }
    public uint Peek()
    {
        return (uint)(array[0] >> 32);
    }
    public int Count => array.Count;
    public bool IsEmpty => array.Count == 0;
}