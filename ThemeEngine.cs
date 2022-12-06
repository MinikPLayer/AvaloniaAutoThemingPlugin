using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Themes.Fluent;

namespace Avalonia.Themes;

public static class ThemeEngine
{
    private static object[] abandonOperation = new object[] { false };

    public static FluentThemeMode CurrentMode
    {
        get
        {
            var fluentStyle = Application.Current.Styles.FirstOrDefault(x => x.GetType() == typeof(FluentTheme)) as FluentTheme;
            if (fluentStyle == null)
                throw new NullReferenceException("Fluent style is not defined");
            return fluentStyle.Mode;
        }
        private set
        {
            var fluentStyle = Application.Current.Styles.FirstOrDefault(x => x.GetType() == typeof(FluentTheme)) as FluentTheme;
            if (fluentStyle == null)
                throw new NullReferenceException("Fluent style is not defined");

            fluentStyle.Mode = value;
        }
    }

    public static Color ToColor(this Vector3 v, byte alpha = 255)
    {
        return Color.FromArgb(alpha, (byte)v.X, (byte)v.Y, (byte)v.Z);
    }

    public static Vector3 ToVector3(this Color c)
    {
        return new Vector3(c.R, c.G, c.B);
    }

    private static async Task BackgroundChange(object[] abandonOperationFlag, Window window, Color target, bool smooth = true)
    {
        var cur = window.Background as SolidColorBrush;
        //ImmutableSolidColorBrush? curI = null;
        if (window.ActualTransparencyLevel != WindowTransparencyLevel.None)
        {
            target = Color.FromArgb(128, target.R, target.G, target.B);
            //curI = window.TransparencyBackgroundFallback as ImmutableSolidColorBrush;
            if(cur == null)
                cur = new SolidColorBrush(Colors.Transparent);
        }

        if (cur == null) //&& curI is null)
        {
            window.Background = new SolidColorBrush(target);
            return;
        }

        //var curColor = cur?.Color ?? curI!.Color;
        var curColor = cur.Color;
        if (smooth)
        {
            var cur3 = curColor.ToVector3();
            var diff = target.ToVector3() - cur3;
            var alphaDiff = target.A - curColor.A;
            const int steps = 20;
            var step = diff / steps;
            var alphaStep = alphaDiff / steps;
            for (int i = 0; i < steps; i++)
            {
                if ((bool)abandonOperationFlag[0])
                    return;
                
                var curStep = cur3 + step * i;

                var newColor = curStep.ToColor((byte)(curColor.A + alphaStep * i));
                window.Background = new SolidColorBrush(newColor);
                await Task.Delay(8);
            }
        }

        window.Background = new SolidColorBrush(target);
    }

    static void InvalidateRunningOperations()
    {
        abandonOperation[0] = true;
        abandonOperation = new object[] { false };
    }
    
    public static async Task ChangeToSystemTheme(Window window, bool smooth = false)
    {
        CurrentMode = PlatformTheme.IsDarkMode() ? FluentThemeMode.Dark : FluentThemeMode.Light;
        
        InvalidateRunningOperations();
        await BackgroundChange(abandonOperation, window, PlatformTheme.GetBackgroundColor(), smooth);
    }
    
    public static async Task ChangeDarkLightMode(Window window, FluentThemeMode newMode)
    {
        CurrentMode = newMode;

        InvalidateRunningOperations();
        await BackgroundChange(abandonOperation, window, newMode == FluentThemeMode.Light ? Colors.White : Colors.Black);
    }
    
    public static async Task FlipDarkLightMode(Window window)
    {
        await ChangeDarkLightMode(window,
            CurrentMode == FluentThemeMode.Dark ? FluentThemeMode.Light : FluentThemeMode.Dark);
    }
}