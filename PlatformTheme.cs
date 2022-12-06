using System;
using Avalonia.Media;

namespace Avalonia.Themes;

public abstract class PlatformTheme
{
    private static PlatformTheme? GetPlatformTheme()
    {
        var os = Environment.OSVersion;
        switch (os.Platform)
        {
            case PlatformID.Unix:
                return new PlatformThemeLinux();
            
            case PlatformID.Win32NT:
                return new PlatformThemeWindows();
            
            default:
                Console.WriteLine($"Platform {os.Platform} not supported");
                return null;
        }
    }
    
    protected abstract Color _GetBackgroundColor();
    protected abstract bool _IsDarkMode();

    public static Color DefaultBackgroundColor = Color.FromRgb(0, 0, 0);
    public static bool DefaultDarkMode = true;
    
    public static Color GetBackgroundColor()
    {
        var platform = GetPlatformTheme();
        return platform?._GetBackgroundColor() ?? DefaultBackgroundColor;
    }
    
    public static bool IsDarkMode()
    {
        var platform = GetPlatformTheme();
        return platform?._IsDarkMode() ?? DefaultDarkMode;
    }
}