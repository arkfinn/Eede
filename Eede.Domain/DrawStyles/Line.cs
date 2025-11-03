using Eede.Domain.Drawing;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Domain.SharedKernel;
using System;

namespace Eede.Domain.DrawStyles
{
    // 実装内容を再検討する
    public record Line : IDrawStyle
    {
        public DrawingBuffer DrawStart(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift)
        {
            Drawer drawer = new(buffer.Previous, penStyle);
            return buffer.UpdateDrawing(drawer.DrawPoint(positionHistory.Now));
        }

        public DrawingBuffer Drawing(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift)
        {
            Drawer drawer = new(buffer.Previous, penStyle);
            return buffer.UpdateDrawing(Draw(drawer, positionHistory, isShift));
        }

        public DrawingBuffer DrawEnd(DrawingBuffer buffer, PenStyle penStyle, PositionHistory positionHistory, bool isShift)
        {
            Drawer drawer = new(buffer.Previous, penStyle);
            return ContextFactory.Create(Draw(drawer, positionHistory, isShift));
        }

        private Picture Draw(Drawer drawer, PositionHistory positionHistory, bool isShift)
        {
            Position to = isShift ? CalculateShiftedPosition(positionHistory.Start, positionHistory.Now) : positionHistory.Now;
            return drawer.DrawLine(positionHistory.Start, to);
        }

        private Position CalculateShiftedPosition(Position beginPos, Position endPos)
        {
            Position margin = new(endPos.X - beginPos.X, endPos.Y - beginPos.Y);
            int deg = (int)Math.Round(((Math.Atan2(margin.Y, margin.X) * 57.29578) + 180) / 22.5);
            Position revise = deg switch
            {
                0 or 15 or 16 => new Position(-1, 0),
                1 or 2 => new Position(-1, -1),
                3 or 4 => new Position(0, -1),
                5 or 6 => new Position(1, -1),
                7 or 8 => new Position(1, 0),
                9 or 10 => new Position(1, 1),
                11 or 12 => new Position(0, 1),
                13 or 14 => new Position(-1, 1),
                _ => throw new InvalidOperationException("不正な計算によるエラー"),
            };

            // 角度別に処理(強引)
            int plusValue = Math.Max(Math.Abs(margin.X), Math.Abs(margin.Y));
            return new Position(
                beginPos.X + (plusValue * revise.X),
                beginPos.Y + (plusValue * revise.Y)
            );
        }
    }
}
