using Eede.Application.PaintLayers;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Positions;
using Eede.Domain.Scales;
using Eede.Domain.Sizes;
using System.Drawing;

namespace Eede.Application
{
    public class PaintArea
    {
        public PaintArea(ICanvasBackgroundService background, Magnification magnification, Size paintSize, Size gridSize)
        {
            Background = background;
            Magnification = magnification;
            RealSize = paintSize;
            DisplaySize = Magnify(paintSize);
            GridSize = gridSize;
        }

        private readonly ICanvasBackgroundService Background;

        private readonly Magnification Magnification;

        private readonly Size GridSize;

        // 実際のサイズ・ポジション RealSize RealPosition
        private readonly Size RealSize;

        // 表示上のサイズ・ポジション DisplaySize DisplayPosition
        public readonly MagnifiedSize DisplaySize;

        private MagnifiedSize Magnify(Size size)
        {
            return new MagnifiedSize(size, Magnification);
        }

        public void Paint(Graphics g, AlphaPicture source, IImageTransfer imageTransfer)
        {
            var backgroundLayer = new PaintBackgroundLayer(Background);
            var bufferLayer = new PaintBufferLayer(DisplaySize, source, imageTransfer);
            var gridLayer = new PaintGridLayer(DisplaySize, Magnify(GridSize));
            backgroundLayer.Paint(g);
            bufferLayer.Paint(g);
            gridLayer.Paint(g);
        }

        public PaintArea UpdateSize(Size paintSize)
        {
            return new PaintArea(Background, Magnification, paintSize, GridSize);
        }

        public PaintArea UpdateMagnification(Magnification m)
        {
            return new PaintArea(Background, m, RealSize, GridSize);
        }

        public MinifiedPosition RealPositionOf(Position position)
        {
            return new MinifiedPosition(position, Magnification);
        }
    }
}