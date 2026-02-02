using Avalonia.Media.Imaging;
using Eede.Domain.Files;
using Eede.Presentation.Common.Models;
using Eede.Application.Infrastructure; // 追加
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
            return "新しいファイル"; // 新規ファイルのサブジェクト�E固宁E
        }

        public override async Task<SaveImageResult> SaveAsync(IFileStorage storage)
        {
            return await SaveWithFilePickerAsync(storage);
        }
    }
}
