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
        private readonly IPictureCodec? _pictureCodec;
        private readonly Func<IFileStorage?> _fileStorageProvider;

        public PictureRepository(
            IBitmapAdapter<Bitmap> bitmapAdapter,
            IPictureCodec? pictureCodec = null,
            Func<IFileStorage?>? fileStorageProvider = null)
        {
            _bitmapAdapter = bitmapAdapter;
            _pictureCodec = pictureCodec;
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
                    return await DecodeStreamAsync(cachedStream, extension);
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
                        return await DecodeStreamAsync(stream, extension);
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
                return await DecodeStreamAsync(fs, extension);
            }

            // 4. 直接パス指定フォールバック
            using var directBitmap = new Bitmap(pathStr);
            return _bitmapAdapter.ConvertToPicture(directBitmap);
        }

        private async Task<Picture> DecodeStreamAsync(Stream stream, string extension)
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            ms.Position = 0;

            if (extension == ".arv")
            {
                ArvFileReader reader = new();
                return reader.Read(ms);
            }

            // PNG形式またはSkiaSharpデコード可能な場合はIPictureCodecを優先（インデックスカラーPNGの色化けやフォーマット反転を完全防止）
            if (_pictureCodec != null)
            {
                try
                {
                    return _pictureCodec.DecodeFromPng(ms.ToArray());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Eede] IPictureCodec decode failed, fallback to Avalonia Bitmap: {ex.Message}");
                }
            }

            ms.Position = 0;
            using var bitmap = new Bitmap(ms);
            return _bitmapAdapter.ConvertToPicture(bitmap);
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
            bitmap.Save(ms, new PngBitmapEncoderOptions());
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
            bitmap.Save(pathStr, new PngBitmapEncoderOptions());
        }
    }
}
