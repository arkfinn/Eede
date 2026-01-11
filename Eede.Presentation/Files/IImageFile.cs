using Avalonia.Media.Imaging;
using Eede.Domain.Files;
using Eede.Presentation.Common.Models;
using Eede.Presentation.Common.Services;
using System.Threading.Tasks;

namespace Eede.Presentation.Files
{
    public interface IImageFile
    {
        Bitmap Bitmap { get; }
        IImageFile WithFilePath(FilePath filePath);
        IImageFile WithBitmap(Bitmap bitmap);
        string Subject();
        Task<SaveImageResult> SaveAsync(IStorageService storage);
    }
}
