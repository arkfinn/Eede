using Eede.Domain.Files;
using Eede.Domain.Pictures;
using System.Drawing;

namespace Eede.Infrastructure.Pictures
{
    public class PictureFileReader : IPictureReader
    {
        public PrimaryPicture Read(FilePath filename)
        {
            using (var image = Image.FromFile(filename.Path))
            {
                return new PrimaryPicture(new Bitmap(image));
            }
        }
    }
}