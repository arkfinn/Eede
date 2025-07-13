using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Eede.Domain.Files;
using Eede.Presentation.Common.Services;

namespace Eede.Presentation.Files
{
    public abstract record AbstractImageFile(Bitmap Bitmap, FilePath Path) : IImageFile
    {
        public IImageFile WithBitmap(Bitmap bitmap) => this with { Bitmap = bitmap };

        public virtual string Subject() => Path.ToString();

        public abstract IImageFile WithFilePath(FilePath filePath);

        public abstract Task<IImageFile> SaveAsync(StorageService storage);

        // 共通のWithFilePathロジック（PNG変換）
        protected IImageFile CreatePngFileWithCheck(Bitmap bitmap, FilePath filePath)
        {
            if (filePath.GetExtension() != ".png")
            {
                throw new ArgumentException("保存ファイルはPNG形式でなければなりません。");
            }
            return new PngFile(bitmap, filePath);
        }

        // 共通のSaveAsyncロジック（ファイルピッカーと保存）
        protected async Task<IImageFile> SaveWithFilePickerAsync(StorageService storage)
        {
            Uri result = await storage.SaveFilePickerAsync();
            if (result == null)
            {
                return this;
            }
            string fullPath = result.LocalPath;

            Bitmap.Save(fullPath);
            return CreatePngFileWithCheck(Bitmap, new FilePath(fullPath));
        }
    }
}
