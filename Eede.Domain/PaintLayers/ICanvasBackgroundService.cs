using System.Drawing;

namespace Eede.Domain.PaintLayers
{
    public interface ICanvasBackgroundService
    {
        void PaintBackground(Graphics g);
    }
}