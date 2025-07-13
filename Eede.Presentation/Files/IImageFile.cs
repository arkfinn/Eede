using Avalonia.Media.Imaging;
using Eede.Domain.Files;

namespace Eede.Presentation.Files
{
    public interface IImageFile
    {
        Bitmap Bitmap { get; }
        FilePath Path { get; }
        bool IsNewFile();
        string GetPathString();
        bool ShouldPromptForSaveAs();
        IImageFile WithFilePath(FilePath filePath);
        IImageFile WithBitmap(Bitmap bitmap);
        string Subject();
    }
}
