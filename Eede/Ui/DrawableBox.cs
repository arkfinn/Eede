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
    public partial class DrawableBox : UserControl, IDrawingArea
    {
        public DrawableBox()
        {
            InitializeComponent();
            var s = GlobalSetting.Instance().BoxSize;
            var gridSize = new Size(16, 16);
            DrawableArea = new DrawableArea(CanvasBackgroundService.Instance, new Magnification(1), gridSize, null);
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

        private DrawableArea DrawableArea;

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

        private PenStyle PenStyle = new PenStyle(new DirectImageBlender());

        public float Magnification
        {
            set
            {
                DrawableArea = DrawableArea.UpdateMagnification(new Magnification(value));
                UpdateCanvasSize();
            }
        }

        public IDrawStyle DrawStyle { private get; set; } = new FreeCurve();

        public Color PenColor
        {
            get => PenStyle.Color;
            set => PenStyle = PenStyle.UpdateColor(value);
        }

        public int PenSize
        {
            set => PenStyle = PenStyle.UpdateWidth(value);
        }

        private void UpdateCanvasSize()
        {
            canvas.Size = DrawableArea.DisplaySizeOf(Buffer.Fetch());
            ResetLocation();
            Refresh();
        }

        public void ChangeImageBlender(IImageBlender b)
        {
            PenStyle = PenStyle.UpdateBlender(b);
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
            DrawableArea.Paint(g, Buffer.Fetch(), imageTransfer);
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

        private void canvas_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    using (var result = DrawableArea.DrawStart(DrawStyle, PenStyle, Buffer, new Position(e.X, e.Y), IsShift()))
                    {
                        UpdatePicture(result.PictureBuffer);
                        DrawableArea = result.DrawableArea;
                    }
                    break;
                //case MouseButtons.Middle:
                //    break;
                //case MouseButtons.None:
                //    break;
                case MouseButtons.Right:
                    if (DrawableArea.IsDrawing())
                    {
                        using (var result = DrawableArea.DrawCancel(Buffer))
                        {
                            UpdatePicture(result.PictureBuffer);
                            DrawableArea = result.DrawableArea;
                        }
                    }
                    else
                    {
                        //色を拾う
                        PenColor = DrawableArea.PickColor(Buffer.Fetch(), new Position(e.X, e.Y));
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

        private bool IsShift()
        {
            return ((Control.ModifierKeys & Keys.Shift) == Keys.Shift);
        }

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            using (var result = DrawableArea.Drawing(DrawStyle, PenStyle, Buffer, new Position(e.X, e.Y), IsShift()))
            {
                UpdatePicture(result.PictureBuffer);
                DrawableArea = result.DrawableArea;
            }
        }

        private void canvas_MouseUp(object sender, MouseEventArgs e)
        {
            using (var result = DrawableArea.DrawEnd(DrawStyle, PenStyle, Buffer, new Position(e.X, e.Y), IsShift()))
            {
                UpdatePicture(result.PictureBuffer);
                DrawableArea = result.DrawableArea;
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