using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Media;

namespace Avalonia.Themes;

public class PlatformThemeLinux : PlatformTheme
{
    protected override Color _GetBackgroundColor()
    {
        // Check for KDE
        const string constFilePath = ".config/kdeglobals";
        const string kdeSearchPattern = "activeBackground=";

        var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), constFilePath);
        
        if (File.Exists(filePath))
        {
            var handle = File.Open(filePath, FileMode.Open);
            using (var reader = new StreamReader(handle))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        continue;
                    
                    if (line.StartsWith(kdeSearchPattern))
                    {
                        line = line.Substring(kdeSearchPattern.Length);
                        var colors = line.Split(',');
                        if (colors.Length != 3)
                        {
                            Debug.WriteLine("Error parsing kdeglobals file");
                            return DefaultBackgroundColor;
                        }

                        byte[] colorBytes = new byte[3];
                        for (int i = 0; i < 3; i++)
                        {
                            if (byte.TryParse(colors[i], out byte b))
                                colorBytes[i] = b;
                            else
                            {
                                Debug.WriteLine("Error parsing kdeglobals file - cannot convert value to byte");
                                return DefaultBackgroundColor;
                            }
                        }

                        return Color.FromRgb(colorBytes[0], colorBytes[1], colorBytes[2]);
                    }
                }
            }
        }
        
        Debug.WriteLine("KDE configuration file doesn't exist, returning default value");
        return DefaultBackgroundColor;
    }

    protected override bool _IsDarkMode()
    {
        var color = this._GetBackgroundColor();
        var sum = color.R + color.B + color.G;
        return sum < (255 * 3) / 2;
    }
}