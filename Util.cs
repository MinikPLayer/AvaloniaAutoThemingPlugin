using System.Numerics;
using Avalonia.Media;

namespace Avalonia.Themes;

public static class Util
{
    public static Color ToColor(this Vector3 v, byte alpha = 255)
    {
        return Color.FromArgb(alpha, (byte)v.X, (byte)v.Y, (byte)v.Z);
    }

    public static Vector3 ToVector3(this Color c)
    {
        return new Vector3(c.R, c.G, c.B);
    }
}