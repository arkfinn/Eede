﻿using Eede.Domain.Pictures;
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
                // TODO: ドットの打ち方は調整したい。1to 4ドット辺りまではrectで
                g.CompositingMode = CompositingMode.SourceCopy;
                g.DrawLine(PenStyle.PreparePen(), new PointF((float)x, (float)y), new PointF((float)x + 0.001f, (float)y + 0.01f));
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