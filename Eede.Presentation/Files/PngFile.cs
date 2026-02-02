using Avalonia.Media.Imaging;
using Eede.Domain.Files;
using Eede.Presentation.Common.Models;
using Eede.Application.Infrastructure;
using System;
using System.Threading.Tasks;

namespace Eede.Presentation.Files
{
    public record PngFile(Bitmap Bitmap, FilePath Path) : AbstractImageFile(Bitmap, Path)
    {
        public override IImageFile WithFilePath(FilePath filePath)
        {
            return this with { Path = filePath };
        }

        public override async Task<SaveImageResult> SaveAsync(IFileStorage storage)
        {
            string fullPath = Path.ToString();
            try
            {
                Bitmap.Save(fullPath);
                return await Task.FromResult(SaveImageResult.Saved(this)); // 保存�E劁E
            }
            catch (Exception)
            {
                return await Task.FromResult(SaveImageResult.Canceled()); // エラーが発生した場吁E
            }
        }
    }
}
