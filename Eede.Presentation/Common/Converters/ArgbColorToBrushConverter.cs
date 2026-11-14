using Avalonia.Data.Converters;
using Avalonia.Media;
using Eede.Domain.Palettes;
using System;
using System.Globalization;

namespace Eede.Presentation.Common.Converters;

#nullable enable

public class ArgbColorToBrushConverter : IValueConverter
{
    public static readonly ArgbColorToBrushConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ArgbColor c)
        {
            return new SolidColorBrush(Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue));
        }
        return Brushes.Transparent;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SolidColorBrush brush)
        {
            Color c = brush.Color;
            return new ArgbColor(c.A, c.R, c.G, c.B);
        }
        return null;
    }
}

public class ArgbColorToHexConverter : IValueConverter
{
    public static readonly ArgbColorToHexConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ArgbColor c)
        {
            return $"#{c.Alpha:X2}{c.Red:X2}{c.Green:X2}{c.Blue:X2}";
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
