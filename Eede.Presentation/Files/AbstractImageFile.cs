using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Eede.Domain.Files;
using Eede.Presentation.Common.Models;
using Eede.Presentation.Common.Services;

namespace Eede.Presentation.Files
{
    public abstract record AbstractImageFile(Bitmap Bitmap, FilePath Path) : IImageFile
    {
        public IImageFile WithBitmap(Bitmap bitmap) => this with { Bitmap = bitmap };

        public virtual string Subject() => Path.ToString();

        public abstract IImageFile WithFilePath(FilePath filePath);

        public abstract Task<SaveImageResult> SaveAsync(IStorageService storage);

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
        protected async Task<SaveImageResult> SaveWithFilePickerAsync(IStorageService storage)
        {
            Uri? result = await storage.SaveFilePickerAsync();
            if (result == null)
            {
                return SaveImageResult.Canceled(); // キャンセルされた場合
            }
            string fullPath = result.LocalPath;

            try
            {
                Bitmap.Save(fullPath);
                return SaveImageResult.Saved(CreatePngFileWithCheck(Bitmap, new FilePath(fullPath))); // 保存成功
            }
            catch (Exception)
            {
                return SaveImageResult.Canceled(); // エラーが発生した場合
            }
        }
    }
}
