using Eede.Domain.Pictures;
using Eede.Domain.Scales;

namespace Eede.Domain.ImageTransfers
{
    public interface IImageTransfer
    {
        Picture Transfer(Picture from, Magnification magnification);
    }
}