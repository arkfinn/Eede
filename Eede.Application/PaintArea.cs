using Eede.Application.PaintLayers;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Domain.Scales;
using Eede.Domain.Sizes;
using System.Drawing;

namespace Eede.Application
{
    public class PaintArea
    {
        public PaintArea(ICanvasBackgroundService background, Magnification magnification, Size gridSize)
        {
            Background = background;
            Magnification = magnification;
            GridSize = gridSize;
        }

        private readonly ICanvasBackgroundService Background;

        private readonly Magnification Magnification;

        private readonly Size GridSize;

        private MagnifiedSize Magnify(Size size)
        {
            return new MagnifiedSize(size, Magnification);
        }

        public void Paint(Graphics g, Picture source, IImageTransfer imageTransfer)
        {
            // 表示上のサイズ・ポジション DisplaySize DisplayPosition
            var displaySize = Magnify(source.Size);
            var backgroundLayer = new PaintBackgroundLayer(Background);
            var bufferLayer = new PaintBufferLayer(displaySize, source, imageTransfer);
            var gridLayer = new PaintGridLayer(displaySize, Magnify(GridSize));
            backgroundLayer.Paint(g);
            bufferLayer.Paint(g);
            gridLayer.Paint(g);
        }

        public PaintArea UpdateMagnification(Magnification m)
        {
            return new PaintArea(Background, m, GridSize);
        }

        public MinifiedPosition RealPositionOf(Position position)
        {
            return new MinifiedPosition(position, Magnification);
        }

        public Size DisplaySizeOf(Picture picture)
        {
            return Magnify(picture.Size).ToSize();
        }

        public Color PickColor(Picture picture, Position pos)
        {
            return picture.PickColor(RealPositionOf(pos).ToPosition());
        }
    }
}