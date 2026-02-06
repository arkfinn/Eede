using Avalonia.Media.Imaging;
using Eede.Domain.Files;
using Eede.Presentation.Common.Models;
using Eede.Application.Infrastructure;
using System;
using System.Threading.Tasks;

namespace Eede.Presentation.Files
{
    public record NewFile : AbstractImageFile
    {
        public NewFile(Bitmap Bitmap) : base(Bitmap, FilePath.Empty())
        {
        }

        public override IImageFile WithFilePath(FilePath filePath)
        {
            return CreatePngFileWithCheck(Bitmap, filePath);
        }

        public override string Subject()
        {
            return "新しいファイル";
        }

        public override async Task<SaveImageResult> SaveAsync(IFileStorage storage)
        {
            // 新規ファイルは保存先がないためピッカーを表示
            return await SaveWithFilePickerAsync(storage);
        }
    }
}