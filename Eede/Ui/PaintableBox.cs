using Eede.Application;
using Eede.Application.Drawings;
using Eede.Domain.DrawStyles;
using Eede.Domain.ImageBlenders;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
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
            PaintArea = new PaintArea(CanvasBackgroundService.Instance, m, gridSize);
            SetupImage(new Bitmap(s.Width, s.Height));
            canvas.Image = Buffer.Fetch().ToImage();
            Disposed += (sender, args) =>
            {
                if (Buffer != null)
                {
                    Buffer.Dispose();
                }
            };
        }

        private PaintArea PaintArea;

        private DrawingBuffer Buffer;

        public Size DrawingSize
        {
            get
            {
                return Buffer.Fetch().Size;
            }
        }

        public Bitmap GetImage()
        {
            return Buffer.Fetch().CutOut(new Rectangle(new Point(0, 0), Buffer.Fetch().Size));
        }

        public void SetupImage(Bitmap image)
        {
            using (var picture = new Picture(image))
            {
                UpdatePicture(new DrawingBuffer(picture));
            }
        }

        private void UpdatePicture(DrawingBuffer newPicture)
        {
            var oldPicture = Buffer;
            Buffer = newPicture.Clone();
            if (oldPicture != null)
            {
                oldPicture.Dispose();
            }
            UpdateCanvasSize();
            canvas.Invalidate();
        }

        private PenStyle mPen;

        private PenStyle PenCase
        {
            get
            {
                if (mPen == null)
                {
                    mPen = new PenStyle(new Pen(Color.Black), new DirectImageBlender());
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
                UpdateCanvasSize();
            }
        }

        public IDrawStyle PenStyle { get; set; } = new FreeCurve();

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

        private void UpdateCanvasSize()
        {
            canvas.Size = PaintArea.DisplaySizeOf(Buffer.Fetch());
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
            PaintArea.Paint(g, Buffer.Fetch(), imageTransfer);
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
                    var runner = new DrawingRunner(PenStyle, PenCase);
                    using (var result = runner.DrawStart(Buffer, PaintArea, new Position(e.X, e.Y), IsShift()))
                    {
                        UpdatePicture(result.PictureBuffer);
                        DrawingRunner = result.Runner;
                    }
                    break;
                //case MouseButtons.Middle:
                //    break;
                //case MouseButtons.None:
                //    break;
                case MouseButtons.Right:
                    if (DrawingRunner.IsDrawing())
                    {
                        using (var result = DrawingRunner.DrawCancel(Buffer))
                        {
                            UpdatePicture(result.PictureBuffer);
                            DrawingRunner = result.Runner;
                        }
                    }
                    else
                    {
                        //色を拾う
                        SetPenColor(PickColor(new MinifiedPosition(new Position(e.X, e.Y), m)));
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

        private Color PickColor(MinifiedPosition pos)
        {
            return Buffer.Fetch().PickColor(pos.ToPosition());
        }

        private bool IsShift()
        {
            return ((Control.ModifierKeys & Keys.Shift) == Keys.Shift);
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            using (var result = DrawingRunner.Drawing(Buffer, PaintArea, new Position(e.X, e.Y), IsShift()))
            {
                UpdatePicture(result.PictureBuffer);
                DrawingRunner = result.Runner;
            }
        }

        private void canvas_MouseUp(object sender, MouseEventArgs e)
        {
            using (var result = (DrawingRunner.DrawEnd(Buffer, PaintArea, new Position(e.X, e.Y), IsShift())))
            {
                UpdatePicture(result.PictureBuffer);
                DrawingRunner = result.Runner;
            }
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