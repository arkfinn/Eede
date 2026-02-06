using Avalonia.Media.Imaging;
using Eede.Domain.Files;
using Eede.Presentation.Common.Models;
using Eede.Application.Infrastructure;
using System;
using System.Threading.Tasks;

namespace Eede.Presentation.Files
{
    public record ArvFile : AbstractImageFile
    {
        public ArvFile(Bitmap Bitmap, FilePath Path) : base(Bitmap, Path)
        {
            if (!Path.IsEmpty() && Path.GetExtension() != ".arv")
            {
                throw new ArgumentException("ArvFile must have .arv extension.", nameof(Path));
            }
        }

        public override IImageFile WithFilePath(FilePath filePath)
        {
            // ArvFileから別のパスに切り替える際は、システム的にPNGへの変換を強制する
            return CreatePngFileWithCheck(Bitmap, filePath);
        }

        public override async Task<SaveImageResult> SaveAsync(IFileStorage storage)
        {
            // ArvFileは常に「名前を付けて保存（PNG化）」を促す（上書き保存を許可しない）
            return await SaveWithFilePickerAsync(storage);
        }
    }
}
