using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Eede.Application.Pictures;
using Eede.Domain.Files;
using Eede.Domain.Pictures;
using Eede.Domain.SharedKernel;
using Eede.Presentation.Common.Adapters;
using System;
using System.IO;
using System.Web;

namespace Eede.Presentation.Files
{
    public class BitmapFileReader
    {
        public IImageFile Read(Uri path)
        {
            try
            {
                string decodedPath = HttpUtility.UrlDecode(path.AbsolutePath);
                FilePath filePath = new(decodedPath);
                string extension = filePath.GetExtension();

                switch (extension.ToLowerInvariant())
                {
                    case ".arv":
                        using (FileStream fs = new(filePath.ToString(), FileMode.Open, FileAccess.Read))
                        {
                            ArvFileReader reader = new();
                            Picture picture = reader.Read(fs);
                            return new ArvFile(PictureBitmapAdapter.ConvertToBitmap(picture), filePath);
                        }
                    case ".png":
                        return new PngFile(new Bitmap(filePath.ToString()), filePath);
                    default:
                        return new BitmapFile(new Bitmap(filePath.ToString()), filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading image file: {ex.Message}");
                return null;
            }
        }

        public static IImageFile CreateEmptyBitmapFile(PictureSize size)
        {
            return new NewFile(
                new WriteableBitmap(
                    new PixelSize(size.Width, size.Height),
                    new Vector(96, 96),
                    PixelFormat.Bgra8888
                )
            );
        }
    }
}

