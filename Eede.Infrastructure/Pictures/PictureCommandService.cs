using Eede.Domain.Files;
using Eede.Domain.Pictures;
using System.Drawing;

namespace Eede.Infrastructure.Pictures
{
    public class PictureCommandService : IPictureCommandService
    {
        public void Save(FilePath path, Bitmap picture)
        {
            if (path.IsEmpty())
            {
                return;
            }
            picture.Save(path.Path, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}