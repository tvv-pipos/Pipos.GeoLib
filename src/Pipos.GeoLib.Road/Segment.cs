using NetTopologySuite.Geometries;

namespace Pipos.GeoLib.Road;

public class Segment
{
    public float X1 { get; set; }
    public float Y1 { get; set; }
    public float X2 { get; set; }
    public float Y2 { get; set; }
    public Edge Edge { get; set; }

    public Segment(float x1, float y1, float x2, float y2, Edge edge)
    {
        X1 = x1;
        Y1 = y1;
        X2 = x2;
        Y2 = y2;
        Edge = edge;
    }

    public Envelope GetEnvelope()
    {
        return new Envelope(X1, X2, Y1, Y2);
    }

    public float Length() 
    {
        float dx = (X2 - X1);
        float dy = (Y2 - Y1);
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    public float DistanceFromPoint(float x, float y)
    {
        float vx = X2 - X1;
        float vy = Y2 - Y1;
        float ux = x - X1;
        float uy = y - Y1;        
        //  v (dot) u / abs(v) == Scalar projection 
        float sproj = (vx * ux + vy * uy) / (vx * vx + vy * vy);
        if(sproj >= 0.0f && sproj <= 1.0f)
        {
            float dx = ux - sproj * vx;
            float dy = uy - sproj * vy;
            return MathF.Sqrt(dx * dx + dy * dy);
        }
        else
        {
            float d1 = (x - X1) * (x - X1) + (y - Y1) * (y - Y1);
            float d2 = (x - X2) * (x - X2) + (y - Y2) * (y - Y2);
            if(d1 < d2)
            {
                return MathF.Sqrt(d1);
            }
            else 
            {
                return MathF.Sqrt(d2);
            }
        }  
    }

    public void DistanceAndPosFromPoint(float x, float y, out float distance, out float posX, out float posY)
    {
        float vx = X2 - X1;
        float vy = Y2 - Y1;
        float ux = x - X1;
        float uy = y - Y1; 
        //  v (dot) u / abs(v) == Scalar projection 
        float sproj = (vx * ux + vy * uy) / (vx * vx + vy * vy);
        if(sproj >= 0.0f && sproj <= 1.0f)
        {
            float dx = ux - sproj * vx;
            float dy = uy - sproj * vy;
            distance = MathF.Sqrt(dx * dx + dy * dy);
            posX = sproj * vx + X1;
            posY = sproj * vy + Y1;
        }
        else
        {
            float d1 = (x - X1) * (x - X1) + (y - Y1) * (y - Y1);
            float d2 = (x - X2) * (x - X2) + (y - Y2) * (y - Y2);
            if(d1 < d2)
            {
                distance = MathF.Sqrt(d1);
                posX = X1;
                posY = Y1;
            }
            else 
            {
                distance = MathF.Sqrt(d2);
                posX = X2;
                posY = Y2;
            }
        }        
    }
}