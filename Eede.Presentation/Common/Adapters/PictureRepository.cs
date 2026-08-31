using Avalonia.Media.Imaging;
using Eede.Application.Infrastructure;
using Eede.Application.Pictures;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Infrastructure.ImageEditing.Persistence.ArvFileFormat;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Eede.Presentation.Common.Adapters
{
    public class PictureRepository : IPictureRepository
    {
        private readonly IBitmapAdapter<Bitmap> _bitmapAdapter;
        private readonly Func<IFileStorage?> _fileStorageProvider;

        public PictureRepository(IBitmapAdapter<Bitmap> bitmapAdapter, Func<IFileStorage?>? fileStorageProvider = null)
        {
            _bitmapAdapter = bitmapAdapter;
            _fileStorageProvider = fileStorageProvider ?? (() => null);
        }

        public async Task<Picture> LoadAsync(FilePath path)
        {
            if (path == null || path.IsEmpty())
            {
                throw new ArgumentException("Path cannot be empty for LoadAsync.");
            }

            string pathStr = path.ToString();
            string extension = path.GetExtension().ToLowerInvariant();

            // 1. 静的キャッシュからブラウザ/選択済みファイルを読み込む（最優先）
            var cachedStream = await AvaloniaFileStorage.TryOpenReadStreamStaticAsync(pathStr);
            if (cachedStream != null)
            {
                await using (cachedStream)
                {
                    using var ms = new MemoryStream();
                    await cachedStream.CopyToAsync(ms);
                    ms.Position = 0;

                    if (extension == ".arv")
                    {
                        ArvFileReader reader = new();
                        return reader.Read(ms);
                    }
                    try
                    {
                        using var bitmap = new Bitmap(ms);
                        return _bitmapAdapter.ConvertToPicture(bitmap);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Eede] Error decoding bitmap from cached stream: {ex}");
                        throw;
                    }
                }
            }

            // 2. IFileStorage 経由で Stream を開く
            if (Uri.TryCreate(pathStr, UriKind.RelativeOrAbsolute, out var uri))
            {
                var fileStorage = _fileStorageProvider();
                if (fileStorage != null)
                {
                    try
                    {
                        await using var stream = await fileStorage.OpenReadStreamAsync(uri);
                        using var ms = new MemoryStream();
                        await stream.CopyToAsync(ms);
                        ms.Position = 0;

                        if (extension == ".arv")
                        {
                            ArvFileReader reader = new();
                            return reader.Read(ms);
                        }
                        using var bitmap = new Bitmap(ms);
                        return _bitmapAdapter.ConvertToPicture(bitmap);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Eede] FileStorage fallback exception: {ex.Message}");
                    }
                }
            }

            // 3. 物理ローカルファイルが存在する場合
            if (File.Exists(pathStr))
            {
                await using var fs = new FileStream(pathStr, FileMode.Open, FileAccess.Read);
                if (extension == ".arv")
                {
                    ArvFileReader reader = new();
                    return reader.Read(fs);
                }
                using var bitmap = new Bitmap(fs);
                return _bitmapAdapter.ConvertToPicture(bitmap);
            }

            // 4. 直接パス指定フォールバック
            using var directBitmap = new Bitmap(pathStr);
            return _bitmapAdapter.ConvertToPicture(directBitmap);
        }

        public async Task SaveAsync(Picture picture, FilePath path)
        {
            if (path == null || path.IsEmpty())
            {
                throw new ArgumentException("Path cannot be empty for SaveAsync.");
            }

            string pathStr = path.ToString();
            string extension = path.GetExtension().ToLowerInvariant();
            if (extension == ".arv")
            {
                throw new NotImplementedException("Saving in .arv format is not implemented yet.");
            }

            using var bitmap = _bitmapAdapter.ConvertToBitmap(picture);
            using var ms = new MemoryStream();
            bitmap.Save(ms);
            ms.Position = 0;

            // 1. 静的キャッシュ経由で保存（最優先）
            var cachedStream = await AvaloniaFileStorage.TryOpenWriteStreamStaticAsync(pathStr);
            if (cachedStream != null)
            {
                await using (cachedStream)
                {
                    await ms.CopyToAsync(cachedStream);
                    await cachedStream.FlushAsync();
                    return;
                }
            }

            // 2. IFileStorage 経由で保存
            if (Uri.TryCreate(pathStr, UriKind.RelativeOrAbsolute, out var uri))
            {
                var fileStorage = _fileStorageProvider();
                if (fileStorage != null)
                {
                    try
                    {
                        ms.Position = 0;
                        await using var stream = await fileStorage.OpenWriteStreamAsync(uri);
                        await ms.CopyToAsync(stream);
                        await stream.FlushAsync();
                        return;
                    }
                    catch
                    {
                        // フォールバック
                    }
                }
            }

            // 3. 物理ファイルパスに直接保存
            bitmap.Save(pathStr);
        }
    }
}
