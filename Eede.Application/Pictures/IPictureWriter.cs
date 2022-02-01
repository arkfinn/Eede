using Eede.Domain.Pictures;

namespace Eede.Application.Pictures
{
    public interface IPictureWriter
    {
        void Write(Picture picture);
    }
}