using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace CW_hammer.Themes
{
    public enum AppTheme { Default, Ocean, Forest, Sunset, Dark }

    public static class ThemeManager
    {
        private static ResourceDictionary? _themeDict;

        private static readonly Dictionary<AppTheme, (
            string Primary, string Accent,
            string Background, string Surface, string Text)> _themes = new()
        {
            { AppTheme.Default, ("#2C3E50", "#3498DB", "#F5F5F5", "#FFFFFF", "#212121") },
            { AppTheme.Ocean,   ("#040D14", "#0096C7", "#071520", "#0A1E2E", "#E0F4FF") },
            { AppTheme.Forest,  ("#2D6A4F", "#52B788", "#F0F7F4", "#FFFFFF", "#1B3A2D") },
            { AppTheme.Sunset,  ("#120505", "#E8613A", "#1A0808", "#220C0C", "#FFE8E0") },
            { AppTheme.Dark,    ("#1E1E1E", "#BB86FC", "#121212", "#2D2D2D", "#FFFFFF") },
        };

        public static void Apply(AppTheme theme)
        {
            var (primary, accent, background, surface, text) = _themes[theme];

            if (_themeDict == null)
            {
                _themeDict = new ResourceDictionary
                {
                    Source = new Uri("Themes/Default.xaml", UriKind.Relative)
                };
                Application.Current.Resources.MergedDictionaries.Add(_themeDict);
            }

            _themeDict["PrimaryBrush"] = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(primary));
            _themeDict["AccentBrush"] = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(accent));
            _themeDict["BackgroundBrush"] = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(background));
            _themeDict["SurfaceBrush"] = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(surface));
            _themeDict["TextBrush"] = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(text));
        }
    }
}