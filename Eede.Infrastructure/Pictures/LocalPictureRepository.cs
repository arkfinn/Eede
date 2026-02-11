using System;
using System.IO;
using System.Threading.Tasks;
using Eede.Domain.Files;
using Eede.Domain.ImageEditing;
using Eede.Infrastructure.ImageEditing.Persistence.ArvFileFormat;

namespace Eede.Infrastructure.Pictures
{
    public class LocalPictureRepository : IPictureRepository
    {
        public Task<Picture> LoadAsync(FilePath path)
        {
            string extension = path.GetExtension().ToLowerInvariant();
            if (extension == ".arv")
            {
                using (FileStream fs = new(path.ToString(), FileMode.Open, FileAccess.Read))
                {
                    ArvFileReader reader = new();
                    return Task.FromResult(reader.Read(fs));
                }
            }

            // PNG/BMP 等の標準形式については、現在は Presentation 層のアダプターが必要。
            // インフラ層で完結させる場合は SkiaSharp 等の導入が必要なため、
            // ここではサポート外の例外を投げるか、将来の拡張ポイントとして残す。
            throw new NotSupportedException($"Extension {extension} is not supported in Infrastructure layer yet.");
        }

        public Task SaveAsync(Picture picture, FilePath path)
        {
            // 保存ロジックも同様。
            throw new NotImplementedException();
        }
    }
}
