using Avalonia.Media.Imaging;
using Eede.Domain.Files;

namespace Eede.Presentation.Files
{
    public class BitmapFile(Bitmap bitmap, FilePath fullPath)
    {
        public readonly Bitmap Bitmap = bitmap;
        public readonly FilePath Path = fullPath;
    }
}
