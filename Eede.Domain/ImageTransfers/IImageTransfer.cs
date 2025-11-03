using Eede.Domain.ImageEditing;
using Eede.Domain.Pictures;

namespace Eede.Domain.ImageTransfers
{
    public interface IImageTransfer
    {
        Picture Transfer(Picture from, Magnification magnification);
    }
}