using Avalonia.Data.Converters;
using Avalonia.Media;
using Eede.Domain.Colors;
using System;
using System.Globalization;

namespace Eede.Presentation.Common.Converters;

#nullable enable

public class BackgroundColorConverter : IValueConverter
{
    public static readonly BackgroundColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is BackgroundColor backgroundColor)
        {
            var argbColor = backgroundColor.Value;
            return Color.FromArgb(argbColor.Alpha, argbColor.Red, argbColor.Green, argbColor.Blue);
        }
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Color color)
        {
            var argbColor = new ArgbColor(color.A, color.R, color.G, color.B);
            return new BackgroundColor(argbColor);
        }
        return null;
    }
}

#nullable disable
