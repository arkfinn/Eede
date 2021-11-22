using Eede.Application;
using Eede.Domain.ImageBlenders;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Positions;
using Eede.Domain.Scales;
using Eede.PenStyles;
using Eede.Services;
using Eede.Settings;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Eede.Ui
{
    public partial class PaintableBox : UserControl, IDrawingArea
    {
        public PaintableBox()
        {
            InitializeComponent();
            var s = GlobalSetting.Instance().BoxSize;
            var gridSize = new Size(16, 16);
            PaintArea = new PaintArea(CanvasBackgroundService.Instance, m, s, gridSize);
            SetupImage(new Bitmap(s.Width, s.Height));
            canvas.Image = new Bitmap(Buffer.Bmp);
        }

        private PaintArea PaintArea;

        private AlphaPicture Buffer;

        public Size DrawingSize
        {
            get
            {
                return Buffer.Size;
            }
        }

        public Bitmap GetImage()
        {
            return Buffer.Bmp;
        }

        public void SetupImage(Bitmap bmp)
        {
            var old = Buffer;
            Buffer = new AlphaPicture(bmp);
            if (old != null)
            {
                old.Dispose();
            }
            PaintArea = PaintArea.UpdateSize(Buffer.Size);
            RefleshCanvasSize();
        }

        private PenCase mPen;

        private PenCase PenCase
        {
            get
            {
                if (mPen == null)
                {
                    mPen = new PenCase(new Pen(Color.Black), new DirectImageBlender());
                }
                return mPen;
            }
            set { mPen = value; }
        }

        private Magnification m = new Magnification(1);

        public float Magnification
        {
            set
            {
                m = new Magnification(value);
                PaintArea = PaintArea.UpdateMagnification(m);
                RefleshCanvasSize();
            }
        }

        public IPenStyle PenStyle { get; set; } = new FreeCurve();

        public void SetPenColor(Color col)
        {
            PenCase.Color = col;
        }

        public Color GetPenColor()
        {
            return PenCase.Color;
        }

        public void SetPenSize(int size)
        {
            PenCase.Width = size;
        }

        private void RefleshCanvasSize()
        {
            canvas.Size = PaintArea.CanvasSize;
            ResetLocation();
            Refresh();
        }

        public void ChangeImageBlender(IImageBlender b)
        {
            PenCase.Blender = b;
        }

        private IImageTransfer imageTransfer = new DirectImageTransfer();

        public void ChangeImageTransfer(IImageTransfer i)
        {
            imageTransfer = i;
            // PaintArea = PaintArea.UpdateImageTransfer(i);
            Refresh();
        }

        private void PaintUpdate(Graphics g)
        {
            // PaintArea.Paint(g, Buffer);
            PaintArea.Paint(g, Buffer, imageTransfer);
        }

        private void ResetLocation()
        {
            Point now = AutoScrollPosition;
            int newWidth = CalculateCenterPoint(now.X, canvas.Width, Width);
            int newHeight = CalculateCenterPoint(now.Y, canvas.Height, Height);
            canvas.Location = new Point(newWidth, newHeight);
        }

        private int CalculateCenterPoint(int now, int canvasLength, int fullLength)
        {
            if (canvasLength < fullLength)
            {
                return (fullLength / 2) - (canvasLength / 2);
            }
            return now;
        }

        #region イベント

        private void PaintableBox_SizeChanged(object sender, EventArgs e)
        {
            ResetLocation();
        }

        public event EventHandler ColorChanged;

        private void fireColorChanged()
        {
            ColorChanged?.Invoke(this, EventArgs.Empty);
        }

        private PositionHistory PositionHistory = null;
        // PositionHistory = new EmptyPositionHistory();

        private void canvas_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (PositionHistory != null) return;

                    // PositionHistory = PaintArea.BeginDraw(Buffer, new Position(e.X, e.Y), PenStyle, PenCase, IsShift())
                    var pos = new MinifiedPosition(new Position(e.X, e.Y), m).ToPosition();
                    if (Buffer.IsInnerBitmap(pos))
                    {
                        // Bufferを控えておく
                        // beginからfinishまでの間情報を保持するクラス
                        // Positionhistory, BeforeBuffer, PenStyle, PenCase

                        PositionHistory = new PositionHistory(pos);

                        PenStyle.DrawBegin(Buffer, PenCase, PositionHistory, IsShift());
                        canvas.Invalidate();
                    }
                    break;
                //case MouseButtons.Middle:
                //    break;
                //case MouseButtons.None:
                //    break;
                case MouseButtons.Right:
                    if (PositionHistory != null)
                    {
                        //描画をキャンセルする
                        PositionHistory = null;
                        // Bufferを元に戻す
                    }
                    else
                    {
                        //色を拾う
                        SetPenColor(DropColor(new MinifiedPosition(new Position(e.X, e.Y), m)));
                        // TODO: PaintArea.DropColor(Buffer, new Position(e.X, e.Y))
                        fireColorChanged();
                    }
                    break;
                //case MouseButtons.XButton1:
                //    break;
                //case MouseButtons.XButton2:
                //    break;
                default:
                    break;
            }
        }

        private Color DropColor(MinifiedPosition pos)
        {
            var dropper = new ColorDropper(Buffer.Bmp);
            return dropper.Drop(pos.ToPosition());
        }

        private bool IsShift()
        {
            return ((Control.ModifierKeys & Keys.Shift) == Keys.Shift);
        }

        private void UpdatePositionHistory(MinifiedPosition pos)
        {
            PositionHistory = PositionHistory.Update(pos.ToPosition());
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (PositionHistory == null) return;
            // PositionHistory = PaintArea.Drawing(Buffer, PositionHistory, new Position(e.X, e.Y), PenStyle, PenCase, IsShift())

            var pos = new MinifiedPosition(new Position(e.X, e.Y), m);
            UpdatePositionHistory(pos);
            PenStyle.Drawing(Buffer, PenCase, PositionHistory, IsShift());
            canvas.Invalidate();
        }

        private void canvas_MouseUp(object sender, MouseEventArgs e)
        {
            if (PositionHistory == null) return;
            // PositionHistory = PaintArea.FinishDraw(Buffer, PositionHistory, new Position(e.X, e.Y), PenStyle, PenCase, IsShift())

            var pos = new MinifiedPosition(new Position(e.X, e.Y), m);
            UpdatePositionHistory(pos);

            PenStyle.DrawEnd(Buffer, PenCase, PositionHistory, IsShift());
            PositionHistory = null;
        }

        private void canvas_MouseEnter(object sender, EventArgs e)
        {
        }

        private void canvas_MouseLeave(object sender, EventArgs e)
        {
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            PaintUpdate(e.Graphics);
        }

        #endregion イベント
    }
}