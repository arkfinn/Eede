using Eede.Domain.Pictures;
using Eede.Domain.Sizes;
using System.Drawing;

namespace Eede.Domain.ImageTransfers
{
    public interface IImageTransfer
    {
        PictureData Transfer(Bitmap from, MagnifiedSize size);
    }
}