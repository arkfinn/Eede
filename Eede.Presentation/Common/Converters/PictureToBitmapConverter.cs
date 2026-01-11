using Avalonia.Data.Converters;
using Eede.Domain.ImageEditing;
using Eede.Presentation.Common.Adapters;
using System;
using System.Globalization;

namespace Eede.Presentation.Common.Converters
{
    public class PictureToBitmapConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Picture picture)
            {
                return PictureBitmapAdapter.ConvertToPremultipliedBitmap(picture);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
