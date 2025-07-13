using Avalonia.Media.Imaging;
using Eede.Domain.Files;

namespace Eede.Presentation.Files
{
    public record PngFile(Bitmap Bitmap, FilePath Path) : IImageFile
    {
        public bool IsNewFile() => Path.IsEmpty();
        public string GetPathString() => Path.ToString();
        public bool ShouldPromptForSaveAs() => IsNewFile(); // 新規ファイルの場合のみ名前を付けて保存を促す
        public IImageFile WithFilePath(FilePath filePath) => this with { Path = filePath };
        public IImageFile WithBitmap(Bitmap bitmap) => this with { Bitmap = bitmap };
        public string Subject() => GetPathString();
    }
}
