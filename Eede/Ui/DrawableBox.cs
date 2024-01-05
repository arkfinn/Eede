using Eede.Application.Drawings;
using Eede.Application.Pictures;
using Eede.Domain.Colors;
using Eede.Domain.Drawings;
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
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Eede.Ui
{
    public partial class DrawableBox : UserControl, IDrawingArea
    {
        public DrawableBox()
        {
            InitializeComponent();
            Size defaultBoxSize = GlobalSetting.Instance().BoxSize;
            PictureSize gridSize = new(16, 16);
            DrawableArea = new DrawableArea(CanvasBackgroundService.Instance, new Magnification(1), gridSize, null);
            Picture picture = Picture.CreateEmpty(new PictureSize(defaultBoxSize.Width, defaultBoxSize.Height));
            SetupPicture(picture);
        }

        private DrawableArea DrawableArea;

        private DrawingBuffer PictureBuffer;

        public Size DrawingSize
        {
            get
            {
                PictureSize size = PictureBuffer.Fetch().Size;
                return new Size(size.Width, size.Height);
            }
        }

        public Picture GetImage()
        {
            return PictureBuffer.Fetch().CutOut(new PictureArea(new Position(0, 0), new PictureSize(DrawingSize.Width, DrawingSize.Height)));
        }

        public void SetupPicture(Picture sourcePicture)
        {
            UpdatePictureBuffer(new DrawingBuffer(sourcePicture));
        }

        private void UpdatePictureBuffer(DrawingBuffer newPicture)
        {
            PictureBuffer = newPicture.Clone();
            UpdateCanvasSize();
            canvas.Invalidate();
        }

        private PenStyle PenStyle = new(new DirectImageBlender(), new ArgbColor(255, 0, 0, 0), 1);

        public Magnification Magnification
        {
            get => DrawableArea.Magnification;
            set
            {
                DrawableArea = DrawableArea.UpdateMagnification(value);
                UpdateCanvasSize();
            }
        }

        public IDrawStyle DrawStyle { get; set; } = new FreeCurve();

        public ArgbColor PenColor
        {
            get => PenStyle.Color;
            set => PenStyle = PenStyle.UpdateColor(value);
        }

        public int PenSize
        {
            get => PenStyle.Width;
            set => PenStyle = PenStyle.UpdateWidth(value);
        }

        private void UpdateCanvasSize()
        {
            PictureSize size = DrawableArea.DisplaySizeOf(PictureBuffer.Fetch());
            canvas.Size = new(size.Width, size.Height);
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
            return canvasLength < fullLength ? (fullLength / 2) - (canvasLength / 2) : now;
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
            return (Control.ModifierKeys & Keys.Shift) == Keys.Shift;
        }

        public event EventHandler<DrawEventArgs> Drew;

        private void canvas_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    DrawingResult result = DrawableArea.DrawStart(DrawStyle, PenStyle, PictureBuffer, new Position(e.X, e.Y), IsShift());
                    UpdatePictureBuffer(result.PictureBuffer);
                    DrawableArea = result.DrawableArea;
                    break;
                //case MouseButtons.Middle:
                //    break;
                //case MouseButtons.None:
                //    break;
                case MouseButtons.Right:
                    if (PictureBuffer.IsDrawing())
                    {
                        DrawingResult result2 = DrawableArea.DrawCancel(PictureBuffer);
                        UpdatePictureBuffer(result2.PictureBuffer);
                        DrawableArea = result2.DrawableArea;
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
            DrawingResult result = DrawableArea.Move(DrawStyle, PenStyle, PictureBuffer, new Position(e.X, e.Y), IsShift());
            UpdatePictureBuffer(result.PictureBuffer);
            DrawableArea = result.DrawableArea;
        }

        private void canvas_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    Picture previous = PictureBuffer.Previous;

                    DrawingResult result = DrawableArea.DrawEnd(DrawStyle, PenStyle, PictureBuffer, new Position(e.X, e.Y), IsShift());

                    UpdatePictureBuffer(result.PictureBuffer);
                    DrawableArea = result.DrawableArea;
                    Drew?.Invoke(this, new DrawEventArgs(previous, result.PictureBuffer.Previous));

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
            var painted = DrawableArea.Painted(PictureBuffer, PenStyle, imageTransfer);
            using Bitmap src = BitmapConverter.Convert(painted);
            var destination = e.Graphics;
            destination.PixelOffsetMode = PixelOffsetMode.Half;
            destination.InterpolationMode = InterpolationMode.NearestNeighbor;
            destination.DrawImage(src, new Point(0, 0));
        }

    }
}