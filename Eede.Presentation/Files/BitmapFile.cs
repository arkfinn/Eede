using Avalonia.Media.Imaging;
using Eede.Domain.Files;

namespace Eede.Presentation.Files
{
    public record BitmapFile(Bitmap Bitmap, FilePath Path)
    {
        public bool IsNewFile() => Path.IsEmpty();
        public string GetPathString() => Path.ToString();
        public bool IsPngFile() => Path.GetExtension().Equals(".png", System.StringComparison.InvariantCultureIgnoreCase);
        public bool ShouldPromptForSaveAs() => IsNewFile() || !IsPngFile();
        public BitmapFile WithFilePath(FilePath filePath) => this with { Path = filePath };
    }
}
