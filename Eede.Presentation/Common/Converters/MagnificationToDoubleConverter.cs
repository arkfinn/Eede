using Avalonia.Data.Converters;
using Eede.Domain.ImageEditing;
using System;
using System.Globalization;

namespace Eede.Presentation.Common.Converters
{
    public class MagnificationToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Magnification mag)
            {
                return (double)mag.Value;
            }
            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
