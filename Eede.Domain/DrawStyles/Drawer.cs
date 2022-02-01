using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Eede.Domain.DrawStyles
{
    public class Drawer
    {
        public readonly Picture DrawingPicture;

        private readonly PenStyle PenStyle;

        public Drawer(Picture drawingPicture, PenStyle penCase)
        {
            DrawingPicture = drawingPicture;
            PenStyle = penCase;
        }

        public Picture DrawPoint(Position position)
        {
            int x = position.X;
            int y = position.Y;
            return DrawingPicture.Draw(g =>
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                var offset = PenStyle.Width / 2;
                switch (PenStyle.Width)
                {
                    case 1:
                        g.FillRectangle(PenStyle.PrepareBrush(), new Rectangle(x, y, 1, 1));
                        break;

                    case 2:
                    case 3:
                    case 4:
                    case 5:
                        g.FillRectangle(PenStyle.PrepareBrush(), new Rectangle(x - offset, y - offset, PenStyle.Width, PenStyle.Width));
                        break;

                    default:
                        g.FillEllipse(PenStyle.PrepareBrush(), new Rectangle(x - offset, y - offset, PenStyle.Width, PenStyle.Width));
                        break;
                }
            }, PenStyle.Blender);
        }

        public Picture DrawLine(Position from, Position to)
        {
            return DrawingPicture.Draw(g =>
            {
                g.CompositingMode = CompositingMode.SourceCopy;
                g.DrawLine(PenStyle.PreparePen(), from.ToPoint(), to.ToPoint());
            }, PenStyle.Blender);
        }

        public bool Contains(Position position)
        {
            return DrawingPicture.Contains(position);
        }
    }
}