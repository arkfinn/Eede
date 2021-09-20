using Eede.Domain.Files;
using Eede.Domain.Pictures;
using Eede.ImageTransfers;
using Eede.Infrastructure.Pictures;
using Eede.Positions;
using Eede.Services;
using Eede.Settings;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Eede.Ui
{
    public partial class PictureWindow : Form
    {
        /// <summary>
        /// VisualStudioのフォームデザイナー用コンストラクタ
        /// </summary>
        public PictureWindow()
        {
            InitializeComponent();
            SaveTo = new FilePath("");
            SetupPictureBuffer(new PrimaryPicture(new Bitmap(1, 1)));
        }

        public PictureWindow(Size size, IDrawingArea d)
        {
            InitializeComponent();
            var bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.FillRectangle(Brushes.White, new Rectangle(new Point(0, 0), size));
            }
            UpdateSaveTo(new FilePath(""));
            SetupPictureBuffer(new PrimaryPicture(bmp));
            DrawingArea = d;
        }

        public PictureWindow(FilePath filename, IDrawingArea d)
        {
            InitializeComponent();
            UpdateSaveTo(filename);
            var picture = new PictureFileReader().Read(SaveTo);
            SetupPictureBuffer(picture);
            DrawingArea = d;
        }

        private FilePath SaveTo;

        private void UpdateSaveTo(FilePath path)
        {
            SaveTo = path;
            Text = path.IsEmpty() ? "新規ファイル" : path.Path;
        }

        private HalfBoxPosition CursorPosition;

        private IDrawingArea DrawingArea;

        public event EventHandler<BitmapEventArgs> PicturePulled;

        private void InvokePicturePulled(BitmapEventArgs args)
        {
            if (PicturePulled == null) return;
            PicturePulled(this, args);
        }

        public event EventHandler<PicturePushedEventArgs> PicturePushed;

        private void InvokePicturePushed(PicturePushedEventArgs args)
        {
            if (PicturePushed == null) return;
            PicturePushed(this, args);
        }

        private PrimaryPicture PictureBuffer { get; set; }

        private void SetupPictureBuffer(PrimaryPicture picture)
        {
            var old = PictureBuffer;
            PictureBuffer = picture;
            if (old != null)
            {
                old.Dispose();
            }
            ResizePicture(picture.Buffer.Size);
            pictureBox1.Invalidate();
        }

        public void Rename(FilePath path)
        {
            UpdateSaveTo(path);
        }

        public void ResizePicture(Size size)
        {
            var old = pictureBox1.Image;
            pictureBox1.Image = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            if (old != null)
            {
                old.Dispose();
            }
            pictureBox1.Size = size;
            ClientSize = size;
        }

        public void Transfer(Bitmap from, Rectangle rect)
        {
            using (var g = Graphics.FromImage(pictureBox1.Image))
            {
                g.DrawImage(from, rect);
            }
            Refresh();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            CanvasBackgroundService.Instance.PaintBackground(g);

            var transfer = new DirectImageTransfer();
            transfer.Transfer(PictureBuffer.Buffer, g, PictureBuffer.Buffer.Size);
            DrawCursor(g);
        }

        internal bool IsEmptyFileName()
        {
            return SaveTo.IsEmpty();
        }

        private void DrawCursor(Graphics g)
        {
            if (!CursorVisible) return;
            var rect = CursorPosition.CreateRealRectangle(DrawingArea.DrawingSize);
            g.DrawRectangle(new Pen(Color.Yellow, 1), rect);
            var p = new Pen(Color.Red, 1)
            {
                DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
            };
            g.DrawRectangle(p, rect);
        }

        internal void Save()
        {
            new PictureFileWriter(SaveTo).Write(PictureBuffer);
        }

        private bool CursorVisible = false;

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            CursorVisible = IsInnerPictureBox(new Position(e.Location));
            CursorPosition = new HalfBoxPosition(FetchBoxSize(), e.Location);
            pictureBox1.Refresh();
        }

        private bool IsInnerPictureBox(Position p)
        {
            return p.IsInnerOf(pictureBox1.Size);
        }

        private Size FetchBoxSize()
        {
            return GlobalSetting.Instance().BoxSize;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!CursorVisible) return;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    InvokePicturePushed(new PicturePushedEventArgs(PictureBuffer.Buffer, CursorPosition.RealPosition));
                    Refresh();
                    break;

                case MouseButtons.Right:
                    var rect = CursorPosition.CreateRealRectangle(FetchBoxSize());
                    var bmpNew = PictureBuffer.Buffer.Clone(rect, PictureBuffer.Buffer.PixelFormat);
                    try
                    {
                        InvokePicturePulled(new BitmapEventArgs(bmpNew));
                    }
                    finally
                    {
                        bmpNew.Dispose();
                    }

                    break;
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            CursorVisible = false;
            Refresh();
        }
    }

    public class PrimaryPictureArea
    {
        private FilePath FilePath;
        private IPictureWriter Command;
        private IPictureReader Service;

        public PrimaryPictureArea(FilePath filePath, IPictureWriter command, IPictureReader service)
        {
            FilePath = filePath;
            Command = command;
            Service = service;
        }
    }
}