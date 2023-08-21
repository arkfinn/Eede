using Eede.Domain.Positions;
using System;
using System.Drawing;

namespace Eede.Domain.DrawStyles
{
    // 実装内容を再検討する
    public class Line //: IPenStyle
    {
        #region IPenStyle メンバ

        private Bitmap mBuffer;

        public Bitmap Buffer
        {
            get { return mBuffer; }
            set { mBuffer = value; }
        }

        public AlphaPicture DrawStart(AlphaPicture aBitmap, PenStyle pen, PositionHistory positions, bool isShift)
        {
            aBitmap.DrawPoint(pen, positions.Now);
            Buffer = new Bitmap(aBitmap.Bmp);
            return aBitmap;
        }

        public AlphaPicture DrawEnd(AlphaPicture aBitmap, PenStyle pen, PositionHistory positions, bool isShift)
        {
            return aBitmap;
        }

        public AlphaPicture Drawing(AlphaPicture aBitmap, PenStyle pen, PositionHistory positions, bool isShift)
        {
            Position endPoint;
            endPoint = isShift ?
                GetShiftLinePos(positions.Start, positions.Now) :
                positions.Now;

            Rectangle lastRect = new Rectangle(0, 0, 200, 200);

            if (aBitmap.Contains(positions.Now))
            {
                DrawLine(aBitmap, pen, positions.Start, endPoint, lastRect);
            }
            else if (aBitmap.Contains(positions.Start))
            {
                DrawLine(aBitmap, pen, endPoint, positions.Start, lastRect);
            }
            return aBitmap;
        }

        private void DrawLine(AlphaPicture aBitmap, PenStyle pen, Position beginPos, Position endPos, Rectangle lastRect)
        {
            aBitmap.Bmp = new Bitmap(Buffer);
            using (Graphics g = aBitmap.GetGraphics())
            {
                //    g.DrawImageUnscaled(Buffer, 0, 0,Buffer.Width,Buffer.Height);
                //g.DrawLine(pen.PreparePen(), beginPos.ToPoint(), endPos.ToPoint());
            }
        }

        #endregion IPenStyle メンバ

        private Position GetShiftLinePos(Position beginPos, Position endPos)
        {
            Point margin = new Point(endPos.X - beginPos.X, endPos.Y - beginPos.Y);
            int deg = (int)Math.Round(((Math.Atan2(margin.Y, margin.X) * 57.29578) + 180) / 22.5);
            Point revise;
            switch (deg)
            {
                case 0:
                case 15:
                case 16:
                    revise = new Point(-1, 0);
                    break;

                case 1:
                case 2:
                    revise = new Point(-1, -1);
                    break;

                case 3:
                case 4:
                    revise = new Point(0, -1);
                    break;

                case 5:
                case 6:
                    revise = new Point(1, -1);
                    break;

                case 7:
                case 8:
                    revise = new Point(1, 0);
                    break;

                case 9:
                case 10:
                    revise = new Point(1, 1);
                    break;

                case 11:
                case 12:
                    revise = new Point(0, 1);
                    break;

                case 13:
                case 14:
                    revise = new Point(-1, 1);
                    break;

                default:
                    throw new InvalidOperationException("不正な計算によるエラー");
            }

            //角度別に処理(強引)
            int plusValue = Math.Max(Math.Abs(margin.X), Math.Abs(margin.Y));
            return new Position(
                beginPos.X + (plusValue * revise.X),
                beginPos.Y + (plusValue * revise.Y)
            );
        }
    }
}