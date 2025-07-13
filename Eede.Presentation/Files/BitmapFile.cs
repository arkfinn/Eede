using Avalonia.Media.Imaging;
using Eede.Domain.Files;

namespace Eede.Presentation.Files
{
    public record BitmapFile(Bitmap Bitmap, FilePath Path) : IImageFile
    {
        public bool IsNewFile() => Path.IsEmpty();
        public string GetPathString() => Path.ToString();
        public bool ShouldPromptForSaveAs() => IsNewFile();
        public IImageFile WithFilePath(FilePath filePath) => this with { Path = filePath };
        public IImageFile WithBitmap(Bitmap bitmap) => this with { Bitmap = bitmap };
        public string Subject() => GetPathString();
    }
}
