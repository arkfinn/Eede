using Eede.Application;
using Eede.Application.Drawings;
using Eede.Domain.ImageBlenders;
using Eede.Domain.ImageTransfers;
using Eede.Domain.PenStyles;
using Eede.Domain.Positions;
using Eede.Domain.Scales;
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
            Disposed += (sender, args) =>
            {
                if (Buffer != null)
                {
                    Buffer.Dispose();
                }
                DrawingRunner.Dispose();
            };
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

        public void SetupImage(Bitmap image)
        {
            UpdatePicture(new AlphaPicture(image));
        }

        private void UpdatePicture(AlphaPicture newPicture)
        {
            var oldPicture = Buffer;
            Buffer = newPicture;
            // 新・旧が同一インスタンスの場合はDisposeにより問題が生じるため処理を省く。
            if (oldPicture != null && oldPicture != newPicture)
            {
                oldPicture.Dispose();
            }
            PaintArea = PaintArea.UpdateSize(Buffer.Size);
            RefleshCanvasSize();
            canvas.Invalidate();
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
            canvas.Size = PaintArea.DisplaySize.ToSize();
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

        private void FireColorChanged()
        {
            ColorChanged?.Invoke(this, EventArgs.Empty);
        }

        private DrawingRunner DrawingRunner = new DrawingRunner(null, null);

        private void canvas_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    DrawingRunner.Dispose();
                    DrawingRunner = new DrawingRunner(PenStyle, PenCase);
                    UpdatePicture(DrawingRunner.DrawStart(Buffer, PaintArea, new Position(e.X, e.Y), IsShift()));
                    break;
                //case MouseButtons.Middle:
                //    break;
                //case MouseButtons.None:
                //    break;
                case MouseButtons.Right:
                    if (DrawingRunner.IsDrawing())
                    {
                        UpdatePicture(DrawingRunner.DrawCancel());
                    }
                    else
                    {
                        //色を拾う
                        SetPenColor(DropColor(new MinifiedPosition(new Position(e.X, e.Y), m)));
                        // TODO: PaintArea.DropColor(Buffer, new Position(e.X, e.Y))
                        FireColorChanged();
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

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            UpdatePicture(DrawingRunner.Drawing(Buffer, PaintArea, new Position(e.X, e.Y), IsShift()));
        }

        private void canvas_MouseUp(object sender, MouseEventArgs e)
        {
            UpdatePicture(DrawingRunner.DrawEnd(Buffer, PaintArea, new Position(e.X, e.Y), IsShift()));
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