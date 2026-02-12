using Avalonia.Media.Imaging;
using Eede.Domain.Files;
using Eede.Presentation.Common.Models;
using Eede.Application.Infrastructure;
using System.Threading.Tasks;

namespace Eede.Presentation.Files
{
    public interface IImageFile
    {
        Bitmap Bitmap { get; }
        FilePath Path { get; }
        IImageFile WithFilePath(FilePath filePath);
        IImageFile WithBitmap(Bitmap bitmap);
        string Subject();
        Task<SaveImageResult> SaveAsync(IFileStorage storage);
    }
}
