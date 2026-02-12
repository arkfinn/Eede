using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Globalization;
using System.Reflection;

namespace Eede.Presentation.Common.Converters
{
    internal sealed class BitmapConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string path)
            {
                return null;
            }
            Uri? uri = null;

            if (path.StartsWith("avares://"))
            {
                uri = new Uri(path);
            }
            else
            {
                Assembly? assembly = Assembly.GetEntryAssembly();
                if (assembly != null)
                {
                    // string? assemblyName = assembly.GetName().Name;
                    string assemblyName = "Eede.Presentation";
                    if (assemblyName != null)
                    {
                        uri = new Uri($"avares://{assemblyName}{path}");
                    }
                }
            }
            if (uri != null)
            {
                using System.IO.Stream stream = AssetLoader.Open(uri);
                return new Bitmap(stream);
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
