using Eede.Domain.ImageEditing;

namespace Eede.Application.Pictures
{
    public interface IPictureWriter
    {
        void Write(Picture picture);
    }
}