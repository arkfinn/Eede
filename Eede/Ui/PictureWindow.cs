using Eede.Application.Pictures;
using Eede.Domain.Files;
using Eede.Domain.ImageTransfers;
using Eede.Domain.Pictures;
using Eede.Domain.Positions;
using Eede.Infrastructure.Pictures;
using Eede.Services;
using Eede.Settings;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
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
            using var image = new Bitmap(1, 1);
            var picture = BitmapConverter.ConvertBack(image);
            SetupPictureBuffer(picture);
        }

        public PictureWindow(FilePath filename, Picture picture, IDrawingArea d)
        {
            InitializeComponent();
            UpdateSaveTo(filename);
            SetupPictureBuffer(picture);
            DrawingArea = d;
        }

        private FilePath SaveTo;

        private void UpdateSaveTo(FilePath path)
        {
            SaveTo = path;
            Text = path.IsEmpty() ? "新規ファイル" : path.Path;
        }

        private HalfBoxArea CursorPosition;

        private IDrawingArea DrawingArea;

        public event EventHandler<PicturePulledEventArgs> PicturePulled;

        private void InvokePicturePulled(PicturePulledEventArgs args)
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

        private Picture PictureBuffer;

        public void SetupPictureBuffer(Picture picture)
        {
            PictureBuffer = picture;
            ResizePicture(new Size(picture.Size.Width, picture.Size.Height));
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
            var data = PictureBuffer.Transfer(transfer);
            using var dest = BitmapConverter.Convert(data);
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(dest, new Point(0, 0));
            DrawCursor(g);
        }

        internal bool IsEmptyFileName()
        {
            return SaveTo.IsEmpty();
        }

        private void DrawCursor(Graphics g)
        {
            if (!CursorVisible) return;
            PictureArea area;
            if (IsSelecting)
            {
                area = SelectingPosition.CreateRealRectangle(SelectingPosition.BoxSize);
            }
            else
            {
                area = CursorPosition.CreateRealRectangle(new PictureSize(DrawingArea.DrawingSize.Width, DrawingArea.DrawingSize.Height));
            }
            var rect = new Rectangle(area.X, area.Y, area.Width, area.Height);

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

        private bool IsSelecting = false;
        private HalfBoxArea SelectingPosition;

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!CursorVisible) return;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (!IsSelecting)
                    {
                        InvokePicturePushed(new PicturePushedEventArgs(PictureBuffer, CursorPosition.RealPosition));
                        Refresh();
                    }
                    break;

                case MouseButtons.Right:
                    // 範囲選択開始
                    IsSelecting = true;
                    var size = FetchBoxSize();
                    SelectingPosition = HalfBoxArea.Create(new PictureSize(size.Width, size.Height), new Position(e.Location.X, e.Location.Y));
                    break;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsSelecting)
            {
                SelectingPosition = SelectingPosition.UpdatePosition(
                    new Position(e.Location.X, e.Location.Y), 
                    new PictureSize(PictureBuffer.Size.Width, PictureBuffer.Size.Height));
            }
            else
            {
                CursorVisible = IsInnerPictureBox(new Position(e.Location.X, e.Location.Y));
                var size = FetchBoxSize();
                CursorPosition = HalfBoxArea.Create(new PictureSize(size.Width, size.Height), new Position(e.Location.X, e.Location.Y));
            }
            pictureBox1.Refresh();
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    break;

                case MouseButtons.Right:
                    // 範囲選択してるとき
                    if (IsSelecting)
                    {
                        var area = SelectingPosition.CreateRealRectangle(SelectingPosition.BoxSize);
                        var rect = new Rectangle(area.X, area.Y, area.Width, area.Height);
                        InvokePicturePulled(new PicturePulledEventArgs(PictureBuffer, rect));
                        IsSelecting = false;
                    }
                    break;
            }
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