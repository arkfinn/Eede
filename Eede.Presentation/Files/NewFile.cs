using Avalonia.Media.Imaging;
using Eede.Domain.Files;
using Eede.Presentation.Common.Services; // 追加
using System.Threading.Tasks;

namespace Eede.Presentation.Files
{
    public record NewFile(Bitmap Bitmap) : AbstractImageFile(Bitmap, FilePath.Empty())
    {
        public override IImageFile WithFilePath(FilePath filePath)
        {
            return CreatePngFileWithCheck(Bitmap, filePath);
        }

        public override string Subject()
        {
            return "新しいファイル"; // 新規ファイルのサブジェクトは固定
        }

        public override async Task<IImageFile> SaveAsync(StorageService storage)
        {
            return await SaveWithFilePickerAsync(storage);
        }
    }
}
