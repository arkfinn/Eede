using Avalonia.Media.Imaging;
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

        public PictureRepository(IBitmapAdapter<Bitmap> bitmapAdapter)
        {
            _bitmapAdapter = bitmapAdapter;
        }

        public Task<Picture> LoadAsync(FilePath path)
        {
            if (path == null || path.IsEmpty())
            {
                throw new ArgumentException("Path cannot be empty for LoadAsync.");
            }

            string extension = path.GetExtension().ToLowerInvariant();
            if (extension == ".arv")
            {
                using (FileStream fs = new(path.ToString(), FileMode.Open, FileAccess.Read))
                {
                    ArvFileReader reader = new();
                    return Task.FromResult(reader.Read(fs));
                }
            }

            // AvaloniaのBitmapを使用してロード
            using var bitmap = new Bitmap(path.ToString());
            return Task.FromResult(_bitmapAdapter.ConvertToPicture(bitmap));
        }

        public Task SaveAsync(Picture picture, FilePath path)
        {
            if (path == null || path.IsEmpty())
            {
                throw new ArgumentException("Path cannot be empty for SaveAsync.");
            }

            string extension = path.GetExtension().ToLowerInvariant();
            if (extension == ".arv")
            {
                // ARV形式での保存ロジック（未実装の場合は例外を投げるか実装する）
                throw new NotImplementedException("Saving in .arv format is not implemented yet.");
            }

            // PictureをBitmapに変換して保存
            using var bitmap = _bitmapAdapter.ConvertToBitmap(picture);
            bitmap.Save(path.ToString());
            return Task.CompletedTask;
        }
    }
}
