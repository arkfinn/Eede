using Eede.Domain.Pictures;
using Eede.Domain.Files;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.Infrastructure.Pictures
{
    class PictureCommandService : IPictureCommandService
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
