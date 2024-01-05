using Eede.Domain.Pictures;
using System.Drawing;

namespace Eede.Application.PaintLayers
{
    public interface IPaintLayer
    {
        void Paint(Graphics destination);
        Picture Painted(Picture destination);
    }
}