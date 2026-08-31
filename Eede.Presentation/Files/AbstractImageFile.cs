using System;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Eede.Domain.Files;
using Eede.Presentation.Common.Models;
using Eede.Presentation.Common.Adapters;
using Eede.Application.Infrastructure;

namespace Eede.Presentation.Files
{
#nullable enable

    public abstract record AbstractImageFile(Bitmap Bitmap, FilePath Path) : IImageFile
    {
        public IImageFile WithBitmap(Bitmap bitmap) => this with { Bitmap = bitmap };

        public virtual string Subject() => Path.ToString();

        public abstract IImageFile WithFilePath(FilePath filePath);

        public abstract Task<SaveImageResult> SaveAsync(IFileStorage storage);

        public virtual async Task<SaveImageResult> SaveAsAsync(IFileStorage storage)
        {
            return await SaveWithFilePickerAsync(storage);
        }

        // 共通 WithFilePathロジック（PNG変換）
        protected IImageFile CreatePngFileWithCheck(Bitmap bitmap, FilePath filePath)
        {
            if (filePath.GetExtension() != ".png")
            {
                throw new ArgumentException("保存ファイルはPNG形式でなければなりません。");
            }
            return new PngFile(bitmap, filePath);
        }

        protected async Task<SaveImageResult> SaveToPathAsync(FilePath filePath, IFileStorage? storage = null)
        {
            string pathStr = filePath.ToString();
            try
            {
                using var ms = new System.IO.MemoryStream();
                Bitmap.Save(ms);
                ms.Position = 0;

                // 1. 静的キャッシュ経由（ブラウザ環境 / 選択済みファイル）
                var cachedStream = await AvaloniaFileStorage.TryOpenWriteStreamStaticAsync(pathStr);
                if (cachedStream != null)
                {
                    await using (cachedStream)
                    {
                        await ms.CopyToAsync(cachedStream);
                        await cachedStream.FlushAsync();
                        return SaveImageResult.Saved(WithFilePath(filePath));
                    }
                }

                // 2. IFileStorage 経由
                if (storage != null && Uri.TryCreate(pathStr, UriKind.RelativeOrAbsolute, out var uri))
                {
                    try
                    {
                        ms.Position = 0;
                        await using var stream = await storage.OpenWriteStreamAsync(uri);
                        await ms.CopyToAsync(stream);
                        await stream.FlushAsync();
                        return SaveImageResult.Saved(WithFilePath(filePath));
                    }
                    catch
                    {
                        // フォールバック
                    }
                }

                // 3. 物理ファイルパスに直接保存
                Bitmap.Save(pathStr);
                return SaveImageResult.Saved(WithFilePath(filePath));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Eede] Failed to save image to path '{pathStr}': {ex}");
                return SaveImageResult.Canceled();
            }
        }

        // 共通 SaveAsyncロジック（ファイルピッカーと保存）
        protected async Task<SaveImageResult> SaveWithFilePickerAsync(IFileStorage storage)
        {
            Uri? result = await storage.SaveFilePickerAsync();
            if (result == null)
            {
                return SaveImageResult.Canceled();
            }

            string pathStr = result.IsAbsoluteUri ? (result.IsFile ? result.LocalPath : result.ToString()) : result.OriginalString;
            FilePath filePath = new(pathStr);

            try
            {
                using var ms = new System.IO.MemoryStream();
                Bitmap.Save(ms);
                ms.Position = 0;

                await using var stream = await storage.OpenWriteStreamAsync(result);
                await ms.CopyToAsync(stream);
                await stream.FlushAsync();
                return SaveImageResult.Saved(WithFilePath(filePath));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Eede] Failed to save with file picker: {ex}");
                return await SaveToPathAsync(filePath, storage);
            }
        }
    }
}
