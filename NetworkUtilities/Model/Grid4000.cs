namespace Pipos.Common.NetworkUtilities.Model;

public static class Grid4000
{
    /* (sweref 99 / 4000) sweden bounds */
    public const Int32 Size = 4000;
    public const Int32 XMin = Sweden.XMin / Size;
    public const Int32 XMax = Sweden.XMax / Size;
    public const Int32 YMin = Sweden.YMin / Size;
    public const Int32 YMax = Sweden.YMax / Size;
    public const Int32 Width = XMax - XMin;
    public const Int32 Height = YMax - YMin;

    public static Int32 FromId(Int32 id)
    {
        return (((0xFFFF & id) >> 4) - XMin) + ((id >> 20) - YMin) * Width;
    }
    public static Int32 XFromId(Int32 id)
    {
        return ((0xFFFF & id) >> 4) - XMin;
    }
    public static Int32 YFromId(Int32 id)
    {
        return (id >> 20) - YMin;
    }
    public static Int32 XFromCoordiante(Int32 x)
    {
        return (x - Sweden.XMin) / Size;
    }
    public static Int32 YFromCoordiante(Int32 y)
    {
        return (y - Sweden.YMin) / Size;
    }
    public static Int32 FromIdClamp(Int32 id)
    {
        return Math.Clamp((((0xFFFF & id) >> 4) - XMin) + ((id >> 20) - YMin) * Width, 0, Width*Height - 1);
    }
    public static Int32 XFromIdClamp(Int32 id)
    {
        return Math.Clamp((((0xFFFF & id) >> 4) - XMin), 0, Width - 1);
    }
    public static Int32 YFromIdClamp(Int32 id)
    {
        return Math.Clamp(((id >> 20) - YMin), 0, Height - 1);
    }
    public static Int32 XFromCoordianteClamp(Int32 x)
    {
        return Math.Clamp((x - Sweden.XMin) / Size, 0, Width - 1);
    }
    public static Int32 YFromCoordianteClamp(Int32 y)
    {
        return Math.Clamp((y - Sweden.YMin) / Size, 0, Height - 1);
    }
}
