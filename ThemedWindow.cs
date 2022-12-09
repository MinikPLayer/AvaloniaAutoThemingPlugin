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

public class ThemedWindow : Window
{
    private object[] abandonOperation = new object[] { false };

    public static readonly StyledProperty<byte> BackgroundAlphaProperty =
        AvaloniaProperty.Register<ThemedWindow, byte>(nameof(BackgroundAlpha));

    public byte BackgroundAlpha
    {
        get => GetValue(BackgroundAlphaProperty);
        set => SetValue(BackgroundAlphaProperty, value);
    }
        
    public FluentThemeMode CurrentMode
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

    private async Task BackgroundChange(object[] abandonOperationFlag, Color target, bool smooth = true)
    {
        var cur = this.Background as SolidColorBrush;
        //ImmutableSolidColorBrush? curI = null;
        if (this.ActualTransparencyLevel != WindowTransparencyLevel.None && this.TransparencyLevelHint != WindowTransparencyLevel.None)
        {
            target = Color.FromArgb(BackgroundAlpha, target.R, target.G, target.B);
            //curI = window.TransparencyBackgroundFallback as ImmutableSolidColorBrush;
            if(cur == null)
                cur = new SolidColorBrush(Colors.Transparent);
        }

        if (cur == null) //&& curI is null)
        {
            this.Background = new SolidColorBrush(target);
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
                this.Background = new SolidColorBrush(newColor);
                await Task.Delay(8);
            }
        }

        this.Background = new SolidColorBrush(target);
    }

    void InvalidateRunningOperations()
    {
        abandonOperation[0] = true;
        abandonOperation = new object[] { false };
    }
    
    public async Task ChangeToSystemTheme(bool smooth = false)
    {
        CurrentMode = PlatformTheme.IsDarkMode() ? FluentThemeMode.Dark : FluentThemeMode.Light;
        
        InvalidateRunningOperations();
        await BackgroundChange(abandonOperation,PlatformTheme.GetBackgroundColor(), smooth);
    }
    
    public async Task ChangeDarkLightMode(FluentThemeMode newMode)
    {
        CurrentMode = newMode;

        InvalidateRunningOperations();
        await BackgroundChange(abandonOperation, newMode == FluentThemeMode.Light ? Colors.White : Colors.Black);
    }
    
    public async Task FlipDarkLightMode()
    {
        await ChangeDarkLightMode(CurrentMode == FluentThemeMode.Dark ? FluentThemeMode.Light : FluentThemeMode.Dark);
    }

    private bool enableAutoTheme = false;
    public async void EnableAutoTheme(int delay = 1000)
    {
        if (enableAutoTheme)
            return;

        // Wait for initialization to apply properties after derived class InitializeComponent() call
        while (!IsInitialized)
            await Task.Delay(5);

        enableAutoTheme = true;
        while (enableAutoTheme)
        {
            await ChangeToSystemTheme();
            await Task.Delay(delay);
        }
    }

    public void DisableAutoTheme()
    {
        enableAutoTheme = false;
    }

    protected override void OnClosed(EventArgs e)
    {
        enableAutoTheme = false;
    }

    public ThemedWindow()
    {
        EnableAutoTheme();
    }
}