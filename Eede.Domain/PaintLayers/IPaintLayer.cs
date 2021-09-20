using System.Drawing;

namespace Eede.Domain.PaintLayers
{
    public interface IPaintLayer
    {
        void Paint(Graphics destination);
    }
}