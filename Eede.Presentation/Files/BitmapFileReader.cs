using Avalonia.Media.Imaging;
using Eede.Domain.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Eede.Presentation.Files.Pictures
{
    public class BitmapFileReader
    {
        public BitmapFile Read(Uri path)
        {
            var fullPath = HttpUtility.UrlDecode(path.AbsolutePath);
            return new BitmapFile(new Bitmap(fullPath), new FilePath(fullPath));
        }
    }
}
