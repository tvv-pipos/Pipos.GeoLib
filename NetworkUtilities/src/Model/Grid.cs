namespace Pipos.Common.NetworkUtilities.Model;

public class Grid
{
    private readonly Int32 _Size;
    private Int32 _XMin;
    private Int32 _XMax;
    private Int32 _YMin;
    private Int32 _YMax;
    public Int32 Size { get { return _Size; }} 
    public Int32 XMin { get {return _XMin;} set {_XMin = (value / _Size) * _Size;}}
    public Int32 XMax { get {return _XMax;} set {_XMax = (value / _Size) * (_Size + (value % _Size == 0 ? 0 : 1));}}
    public Int32 YMin { get {return _YMin;} set {_YMin = (value / _Size) * _Size;}}
    public Int32 YMax { get {return _YMax;} set {_YMax = (value / _Size) * (_Size + (value % _Size == 0 ? 0 : 1));}}
    public Int32 Width { get { return (_XMax - _XMin) / _Size; }}
    public Int32 Height { get { return (_YMax - _YMin) / _Size; }}

    public Grid(Int32 Size)
    {
        _Size = Size;
    }
    
    /*public Grid(Int32 Size, Bounds bounds)
    {
        _Size = Size;
        XMin = (int)bounds.XMin;
        XMax = (int)bounds.XMax;
        YMin = (int)bounds.YMin;
        YMax = (int)bounds.YMax;       
    }

    public Bounds GetBounds()
    {
        return new Bounds{
            XMin = _XMin,
            XMax = _XMax,
            YMin = _YMin,
            YMax = _YMax
        };
    }*/

    public Int32 IndexFromCoordinate(Int32 x, Int32 y)
    {
        return ((x - _XMin) / _Size) + ((y - _YMin) / _Size) * Width;
    }
    public Int32 XFromCoordiante(Int32 x)
    {
        return (x - _XMin) / _Size;
    }
    public Int32 YFromCoordiante(Int32 y)
    {
        return (y - _YMin) / _Size;
    }
    public Int32 XFromCoordianteClamp(Int32 x)
    {
        return Math.Clamp((x - _XMin) / _Size, 0, Width - 1);
    }
    public Int32 YFromCoordianteClamp(Int32 y)
    {
        return Math.Clamp((y - _YMin) / _Size, 0, Height - 1);
    }
}
