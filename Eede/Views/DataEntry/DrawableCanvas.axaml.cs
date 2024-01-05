using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Eede.Application.Drawings;
using Eede.Domain.Colors;
using Eede.Domain.Drawings;
using Eede.Domain.DrawStyles;
using Eede.Domain.ImageBlenders;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Domain.Scales;
using Eede.Services;
using Eede.ViewModels.DataEntry;
using ReactiveUI;
using System;
using System.Runtime.InteropServices;

namespace Eede.Views.DataEntry
{
    public partial class DrawableCanvas : UserControl, IViewFor<DrawableCanvasViewModel>
    {
        public DrawableCanvas()
        {
            InitializeComponent();
            canvas.PointerPressed += OnCanvasPointerPressed;
            canvas.PointerMoved += OnCvanvasPointerMoved;
            canvas.PointerReleased += OnCanvasPointerReleased;
            canvas.PointerExited += OnCanvasPointerExited;
            canvas.KeyDown += OnKeyDown;
            canvas.KeyUp += OnKeyUp;
            DataContext = ViewModel;


            Size defaultBoxSize = new(32, 32); //GlobalSetting.Instance().BoxSize;
            PictureSize gridSize = new(16, 16);
            DrawableArea = new(CanvasBackgroundService.Instance, new Magnification(1), gridSize, null);
            Picture picture = Picture.CreateEmpty(new PictureSize((int)defaultBoxSize.Width, (int)defaultBoxSize.Height));
            SetupPicture(picture);


        }

        public DrawableCanvasViewModel ViewModel
        {
            get; set;
        } = new DrawableCanvasViewModel();

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = value as DrawableCanvasViewModel;
        }

        private Application.Drawings.DrawableArea DrawableArea;

        private DrawingBuffer PictureBuffer;

        public PictureSize DrawingSize
        {
            get
            {
                return PictureBuffer.Fetch().Size;
            }
        }

        public Picture GetImage()
        {
            return PictureBuffer.Fetch().CutOut(new PictureArea(new Position(0, 0), DrawingSize));
        }

        public void SetupPicture(Picture sourcePicture)
        {
            UpdatePictureBuffer(new DrawingBuffer(sourcePicture));
        }

        private void UpdatePictureBuffer(DrawingBuffer newPicture)
        {
            PictureBuffer = newPicture.Clone();
            UpdateCanvasSize();
            UpdateImage();
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
            background.Width = size.Width;
            background.Height = size.Height;
            canvas.Width = size.Width;
            canvas.Height = size.Height;
            ResetLocation();
            UpdateImage();
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
            UpdateImage();
        }


        public override void Render(DrawingContext context)
        {


            base.Render(context);
        }

        private void UpdateImage()
        {
            Picture source = DrawableArea.Painted(PictureBuffer, PenStyle, imageTransfer);
            byte[] src = source.CloneImage();
            unsafe
            {
                fixed (byte* pByte = src)
                {
                    IntPtr ptr = new IntPtr(pByte);
                    Bitmap bitmap = new Bitmap(
                        PixelFormats.Rgba8888,
                        AlphaFormat.Premul,
                       ptr,
                       new PixelSize(source.Width, source.Height),
                       new Vector(96, 96),
                       source.Stride);
                    ViewModel.MyBitmap = CreateBitmapFromPixelData(src, source.Width, source.Height);
                }
            }
            InvalidateVisual();
        }

        private WriteableBitmap CreateBitmapFromPixelData(byte[] rgbPixelData, int width, int height)
        {
            // Standard may need to change on some devices
            Vector dpi = new Vector(96, 96);
            // the image has no alpha channel
            var bitmap = new WriteableBitmap(new PixelSize(width, height), dpi, Avalonia.Platform.PixelFormat.Bgra8888);
            using (var frameBuffer = bitmap.Lock())
            {
                Marshal.Copy(rgbPixelData, 0, frameBuffer.Address, rgbPixelData.Length);
            }

            return bitmap;
        }

        private void ResetLocation()
        {
            //Point now = AutoScrollPosition;
            //int newWidth = CalculateCenterPoint(now.X, canvas.Width, Width);
            //int newHeight = CalculateCenterPoint(now.Y, canvas.Height, Height);
            //canvas.Location = new Point(newWidth, newHeight);
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

        private bool IsShifted = false;


        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if ((e.KeyModifiers & KeyModifiers.Shift) != 0)
            {
                IsShifted = true;
            }
        }

        private void OnKeyUp(object? sender, KeyEventArgs e)
        {
            if ((e.KeyModifiers & KeyModifiers.Shift) == 0)
            {
                IsShifted = false;
            }
        }

        public event EventHandler<DrawEventArgs> Drew;

        private void OnCanvasPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var pos = e.GetPosition(this.canvas);
            switch (e.GetCurrentPoint(this.canvas).Properties.PointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonPressed:
                    DrawingResult result = DrawableArea.DrawStart(DrawStyle, PenStyle, PictureBuffer, new Position((int)pos.X, (int)pos.Y), IsShifted);
                    UpdatePictureBuffer(result.PictureBuffer);
                    DrawableArea = result.DrawableArea;
                    break;
                //case MouseButtons.Middle:
                //    break;
                //case MouseButtons.None:
                //    break;
                case PointerUpdateKind.RightButtonPressed:
                    if (PictureBuffer.IsDrawing())
                    {
                        DrawingResult result2 = DrawableArea.DrawCancel(PictureBuffer);
                        UpdatePictureBuffer(result2.PictureBuffer);
                        DrawableArea = result2.DrawableArea;
                    }
                    else
                    {
                        PenColor = DrawableArea.PickColor(PictureBuffer.Fetch(), new Position((int)pos.X, (int)pos.Y));
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
            UpdateImage();
        }

        private void OnCvanvasPointerMoved(object? sender, PointerEventArgs e)
        {
            var pos = e.GetPosition(this.canvas);
            DrawingResult result = DrawableArea.Move(DrawStyle, PenStyle, PictureBuffer, new Position((int)pos.X, (int)pos.Y), IsShifted);
            UpdatePictureBuffer(result.PictureBuffer);
            DrawableArea = result.DrawableArea;
            UpdateImage();
        }

        private void OnCanvasPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            switch (e.GetCurrentPoint(this.canvas).Properties.PointerUpdateKind)
            {
                case PointerUpdateKind.LeftButtonReleased:
                    Picture previous = PictureBuffer.Previous;

                    var pos = e.GetPosition(this.canvas);
                    DrawingResult result = DrawableArea.DrawEnd(DrawStyle, PenStyle, PictureBuffer, new Position((int)pos.X, (int)pos.Y), IsShifted);

                    UpdatePictureBuffer(result.PictureBuffer);
                    DrawableArea = result.DrawableArea;
                    Drew?.Invoke(this, new DrawEventArgs(previous, result.PictureBuffer.Previous));

                    break;
            }
            UpdateImage();
        }

        //private void canvas_MouseEnter(object sender, EventArgs e)
        //{
        //}

        private void OnCanvasPointerExited(object? sender, EventArgs e)
        {
            DrawableArea = DrawableArea.Leave(PictureBuffer);
            UpdateImage();
        }

        //private void canvas_Paint(object sender, PaintEventArgs e)
        //{
        //    PaintUpdate(e.Graphics);
        //}
    }
}
