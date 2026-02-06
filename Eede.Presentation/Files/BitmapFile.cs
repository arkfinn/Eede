using Avalonia.Media.Imaging;
using Eede.Domain.Files;
using Eede.Presentation.Common.Models;
using Eede.Application.Infrastructure;
using System;
using System.Threading.Tasks;

namespace Eede.Presentation.Files
{
    public record BitmapFile : AbstractImageFile
    {
        public BitmapFile(Bitmap Bitmap, FilePath Path) : base(Bitmap, Path)
        {
            if (!Path.IsEmpty() && Path.GetExtension() != ".bmp")
            {
                throw new ArgumentException("BitmapFile must have .bmp extension.", nameof(Path));
            }
        }

        public override IImageFile WithFilePath(FilePath filePath)
        {
            return CreatePngFileWithCheck(Bitmap, filePath);
        }

        public override async Task<SaveImageResult> SaveAsync(IFileStorage storage)
        {
            // BMPは上書き非対応。保存時は常にPNGへの変換（ピッカー表示）を強制する
            return await SaveWithFilePickerAsync(storage);
        }
    }
}
