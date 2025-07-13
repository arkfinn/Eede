using Avalonia.Media.Imaging;
using Eede.Domain.Files;
using Eede.Presentation.Common.Services;
using System.Threading.Tasks;

namespace Eede.Presentation.Files
{
    public record PngFile(Bitmap Bitmap, FilePath Path) : AbstractImageFile(Bitmap, Path)
    {
        public override IImageFile WithFilePath(FilePath filePath)
        {
            return this with { Path = filePath };
        }

        public override async Task<IImageFile> SaveAsync(StorageService storage)
        {
            string fullPath = Path.ToString();
            Bitmap.Save(fullPath);
            return await Task.FromResult(this);
        }
    }
}
