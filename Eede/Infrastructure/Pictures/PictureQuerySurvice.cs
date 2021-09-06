using Eede.Domain.Pictures;
using Eede.Domain.Files;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eede.Infrastructure.Pictures
{
    class PictureQuerySurvice : IPictureQueryService
    {
        public PrimaryPicture Fetch(FilePath filename)
        {
            using (var image = Image.FromFile(filename.Path))
            {
                return new PrimaryPicture(filename, new Bitmap(image));
            }
        }
    }
}
