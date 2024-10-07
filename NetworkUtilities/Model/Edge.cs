namespace Pipos.GeoLib.NetworkUtilities.Model;

public class Edge
{
    public Edge(Node source, Node target, int distance, int speedForward, int speedBackward, int timeForward, int timeBackward, bool connectionEdge = false)
    {
        Source = source;
        Target = target;
        Distance = distance;
        ForwardSpeed = speedForward;
        BackwardSpeed = speedBackward;
        ForwardTime = timeForward;
        BackwardTime = timeBackward;
        IsConnectionEdge = connectionEdge;
    }
    public Edge()
    {
        IsConnectionEdge = false;
        Source = null!;
        Target = null!;
    }

    public Node Source { get; set; }
    public Node Target { get; set; }
    public int Distance { get; set; }
    public int ForwardSpeed { get; set; }
    public int BackwardSpeed { get; set; }
    public int ForwardTime { get; set; }
    public int BackwardTime { get; set; }
    public bool IsConnectionEdge { get; set; }

    public Node GetOtherNode(Node node)
    {
        return node == Source ? Target : Source;
    }

    public int GetForwardTime(Node node)
    {
        if (Source == node)
        {
            return ForwardTime;
        }

        if (Target == node)
        {
            return BackwardTime;
        }

        throw new Exception("The given node is not a part of the edge");
    }

    public int GetBackwardTime(Node node)
    {
        if (Source == node)
        {
            return BackwardTime;
        }

        if (Target == node)
        {
            return ForwardTime;
        }

        throw new Exception("The given node is not a part of the edge");
    }

    public void SetForwardTime(Node node, int time)
    {
        if (Source == node)
        {
            ForwardTime = time;
            return;
        }

        if (Target == node)
        {
            BackwardTime = time;
            return;
        }

        throw new Exception("The given node is not a part of the edge");
    }

    public void SetBackwardTime(Node node, int time)
    {
        if (Source == node)
        {
            BackwardTime = time;
            return;
        }

        if (Target == node)
        {
            ForwardTime = time;
            return;
        }

        throw new Exception("The given node is not a part of the edge");
    }

    public void ReplaceNode(Node oldNode, Node newNode)
    {
        if (oldNode.Id == Source.Id)
        {
            Source = newNode;
        }

        if (oldNode.Id == Target.Id)
        {
            Target = newNode;
        }

        if (Target.Id == Source.Id)
        {
            throw new Exception("Edge pointing to same node");
        }
    }
}