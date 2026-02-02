using Avalonia.Media.Imaging;
using Eede.Domain.Files;
using Eede.Presentation.Common.Models;
using Eede.Application.Infrastructure;
using System.Threading.Tasks;

namespace Eede.Presentation.Files
{
    public record BitmapFile(Bitmap Bitmap, FilePath Path) : AbstractImageFile(Bitmap, Path)
    {
        public override IImageFile WithFilePath(FilePath filePath)
        {
            return CreatePngFileWithCheck(Bitmap, filePath);
        }

        public override async Task<SaveImageResult> SaveAsync(IFileStorage storage)
        {
            return await SaveWithFilePickerAsync(storage);
        }
    }
}
