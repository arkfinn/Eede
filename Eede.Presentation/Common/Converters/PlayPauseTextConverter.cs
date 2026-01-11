using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace Eede.Presentation.Common.Converters;

public class PlayPauseTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isTrue)
        {
            if (parameter?.ToString() == "arrow")
            {
                return isTrue ? "▼" : "▲";
            }
            return isTrue ? "Stop" : "Play";
        }
        return "Play";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
