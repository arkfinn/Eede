using Avalonia.Media.Imaging;
using Eede.Domain.Files;
using Eede.Presentation.Common.Models;
using Eede.Application.Infrastructure;
using System;
using System.Threading.Tasks;

namespace Eede.Presentation.Files
{
    public record PngFile : AbstractImageFile
    {
        public PngFile(Bitmap Bitmap, FilePath Path) : base(Bitmap, Path)
        {
            if (!Path.IsEmpty() && Path.GetExtension() != ".png")
            {
                throw new ArgumentException("PngFile must have .png extension.", nameof(Path));
            }
        }

        public override IImageFile WithFilePath(FilePath filePath)
        {
            return this with { Path = filePath };
        }

        public override async Task<SaveImageResult> SaveAsync(IFileStorage storage)
        {
            // 自身の不変条件（Pathが.pngであること）により、安全に上書き保存を試行できる
            if (!Path.IsEmpty())
            {
                return await SaveToPathAsync(Path);
            }
            return await SaveWithFilePickerAsync(storage);
        }
    }
}
