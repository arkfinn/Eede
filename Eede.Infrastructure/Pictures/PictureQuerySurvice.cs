using Eede.Domain.Files;
using Eede.Domain.Pictures;
using System.Drawing;

namespace Eede.Infrastructure.Pictures
{
    public class PictureQuerySurvice : IPictureQueryService
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