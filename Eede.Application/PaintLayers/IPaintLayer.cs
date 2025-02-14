using Eede.Domain.Pictures;

namespace Eede.Application.PaintLayers
{
    public interface IPaintLayer
    {
        Picture Painted(Picture destination);
    }
}
