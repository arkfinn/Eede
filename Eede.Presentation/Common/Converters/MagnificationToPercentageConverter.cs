using Avalonia.Data.Converters;
using Eede.Domain.ImageEditing;
using System;
using System.Globalization;

namespace Eede.Presentation.Common.Converters
{
    public class MagnificationToPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Magnification mag)
            {
                return $"{(int)(mag.Value * 100)}%";
            }
            return "100%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
