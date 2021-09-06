using System.Drawing;
using Eede.Domain.Files;

namespace Eede.Domain.Pictures
{
    public interface IPictureCommandService
    {
        void Save(FilePath path, Bitmap picture);
    }
}