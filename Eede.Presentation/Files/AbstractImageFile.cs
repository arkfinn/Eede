using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Eede.Domain.Files;
using Eede.Presentation.Common.Models;
using Eede.Application.Infrastructure;

namespace Eede.Presentation.Files
{
    public abstract record AbstractImageFile(Bitmap Bitmap, FilePath Path) : IImageFile
    {
        public IImageFile WithBitmap(Bitmap bitmap) => this with { Bitmap = bitmap };

        public virtual string Subject() => Path.ToString();

        public abstract IImageFile WithFilePath(FilePath filePath);

        public abstract Task<SaveImageResult> SaveAsync(IFileStorage storage);

        // 共通 WithFilePathロジック（PNG変換）
        protected IImageFile CreatePngFileWithCheck(Bitmap bitmap, FilePath filePath)
        {
            if (filePath.GetExtension() != ".png")
            {
                throw new ArgumentException("保存ファイルはPNG形式でなければなりません。");
            }
            return new PngFile(bitmap, filePath);
        }

        protected async Task<SaveImageResult> SaveToPathAsync(FilePath filePath)
        {
            try
            {
                Bitmap.Save(filePath.ToString());
                return SaveImageResult.Saved(WithFilePath(filePath)); // 保存完了
            }
            catch (Exception)
            {
                return SaveImageResult.Canceled(); // エラーが発生した場合
            }
        }

        // 共通 SaveAsyncロジック（ファイルピッカーと保存）
        protected async Task<SaveImageResult> SaveWithFilePickerAsync(IFileStorage storage)
        {
            Uri? result = await storage.SaveFilePickerAsync();
            if (result == null)
            {
                return SaveImageResult.Canceled(); // キャンセルされた場合
            }
            return await SaveToPathAsync(new FilePath(result.LocalPath));
        }
    }
}