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
            var defaultBoxSize = GlobalSetting.Instance().BoxSize;
            var gridSize = new Size(16, 16);
            DrawableArea = new DrawableArea(CanvasBackgroundService.Instance, new Magnification(1), gridSize, null);
            using (var picture = new Picture(defaultBoxSize))
            {
                SetupPicture(picture);
            }
            canvas.Image = PictureBuffer.Fetch().ToImage();
            Disposed += (sender, args) =>
            {
                if (PictureBuffer != null)
                {
                    PictureBuffer.Dispose();
                }
            };
        }

        private DrawableArea DrawableArea;

        private DrawingBuffer PictureBuffer;

        public Size DrawingSize
        {
            get
            {
                return PictureBuffer.Fetch().Size;
            }
        }

        public Bitmap GetImage()
        {
            return PictureBuffer.Fetch().CutOut(new Rectangle(new Point(0, 0), PictureBuffer.Fetch().Size));
        }

        public void SetupPicture(Picture sourcePicture)
        {
            UpdatePictureBuffer(new DrawingBuffer(sourcePicture));
        }

        private void UpdatePictureBuffer(DrawingBuffer newPicture)
        {
            var oldPicture = PictureBuffer;
            PictureBuffer = newPicture.Clone();
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
            canvas.Size = DrawableArea.DisplaySizeOf(PictureBuffer.Fetch());
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
            DrawableArea.Paint(g, PictureBuffer, PenStyle, imageTransfer);
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

        private void PaintableBox_SizeChanged(object sender, EventArgs e)
        {
            ResetLocation();
        }

        public event EventHandler ColorChanged;

        private void FireColorChanged()
        {
            ColorChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool IsShift()
        {
            return ((Control.ModifierKeys & Keys.Shift) == Keys.Shift);
        }

        public event EventHandler<DrawEventArgs> Drew;

        private void canvas_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    using (var result = DrawableArea.DrawStart(DrawStyle, PenStyle, PictureBuffer, new Position(e.X, e.Y), IsShift()))
                    {
                        UpdatePictureBuffer(result.PictureBuffer);
                        DrawableArea = result.DrawableArea;
                    }
                    break;
                //case MouseButtons.Middle:
                //    break;
                //case MouseButtons.None:
                //    break;
                case MouseButtons.Right:
                    if (PictureBuffer.IsDrawing())
                    {
                        using (var result = DrawableArea.DrawCancel(PictureBuffer))
                        {
                            UpdatePictureBuffer(result.PictureBuffer);
                            DrawableArea = result.DrawableArea;
                        }
                    }
                    else
                    {
                        //色を拾う
                        PenColor = DrawableArea.PickColor(PictureBuffer.Fetch(), new Position(e.X, e.Y));
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

        private void canvas_MouseMove(object sender, MouseEventArgs e)
        {
            using (var result = DrawableArea.Move(DrawStyle, PenStyle, PictureBuffer, new Position(e.X, e.Y), IsShift()))
            {
                UpdatePictureBuffer(result.PictureBuffer);
                DrawableArea = result.DrawableArea;
            }
        }

        private void canvas_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    using (var previous = PictureBuffer.Previous.Clone())
                    {
                        using (var result = DrawableArea.DrawEnd(DrawStyle, PenStyle, PictureBuffer, new Position(e.X, e.Y), IsShift()))
                        {
                            UpdatePictureBuffer(result.PictureBuffer);
                            DrawableArea = result.DrawableArea;
                            Drew?.Invoke(this, new DrawEventArgs(previous, result.PictureBuffer.Previous));
                        }
                    }
                    break;
            }
        }

        private void canvas_MouseEnter(object sender, EventArgs e)
        {
        }

        private void canvas_MouseLeave(object sender, EventArgs e)
        {
            DrawableArea = DrawableArea.Leave(PictureBuffer);
            canvas.Invalidate();
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            PaintUpdate(e.Graphics);
        }

    }
}