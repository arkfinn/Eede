using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Eede.Common.Converters
{
    sealed class BitmapConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string path)
            {
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
                        string? assemblyName = "Eede";
                        if (assemblyName != null)
                        {
                            uri = new Uri($"avares://{assemblyName}{path}");
                        }
                    }
                }
                if (uri != null)
                {
                    using var stream = AssetLoader.Open(uri);
                    return new Bitmap(stream);
                }
            }
            return null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
