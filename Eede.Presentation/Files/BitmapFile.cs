using Avalonia.Media.Imaging;
using Eede.Domain.Files;

namespace Eede.Presentation.Files
{
    public record BitmapFile(Bitmap Bitmap, FilePath Path)
    {
        public bool IsNewFile() => Path.IsEmpty();
        public string GetPathString() => Path.Path;
    }
}
