using Eede.Domain.ImageEditing;

namespace Eede.Application.PaintLayers
{
    public interface IPaintLayer
    {
        Picture Painted(Picture destination);
    }
}
