using Avalonia.Media.Imaging;
using Eede.Domain.Files;
using System;
using System.Web;

namespace Eede.Presentation.Files
{
    public class BitmapFileReader
    {
        public BitmapFile Read(Uri path)
        {
            string fullPath = HttpUtility.UrlDecode(path.AbsolutePath);
            return new BitmapFile(new Bitmap(fullPath), new FilePath(fullPath));
        }
    }
}
