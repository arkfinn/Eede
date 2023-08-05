using Eede.Domain.Pictures;
using Eede.Domain.Scales;

namespace Eede.Domain.ImageTransfers
{
    public interface IImageTransfer
    {
        PictureData Transfer(PictureData from, Magnification magnification);
    }
}