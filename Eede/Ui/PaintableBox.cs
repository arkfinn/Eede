using Eede.ImageBlenders;
using Eede.ImageTransfers;
using Eede.PaintLayers;
using Eede.PenStyles;
using Eede.Positions;
using Eede.Services;
using Eede.Settings;
using Eede.Sizes;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Eede.Ui
{
    public partial class PaintableBox : UserControl, IDrawingArea
    {
        public PaintableBox()
        {
            InitializeComponent();
            PaintAreaSize = new MagnifiedSize(new Size(32, 32), m);
            var s = GlobalSetting.Instance().BoxSize;
            SetupImage(new Bitmap(s.Width, s.Height));
            canvas.Image = new Bitmap(Buffer.Bmp);
        }

        private MagnifiedSize PaintAreaSize { get; set; } 

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
            SetSize();
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
                SetSize();
            }
        }
        public IPenStyle PenStyle { get; set; } = new FreeCurve();
        protected Bitmap DrawBuffer { get; set; }

        private bool IsDrawing = false;


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

        private void SetSize()
        {
            if (Buffer == null) return;

            //pictureboxのサイズをセット
            this.PaintAreaSize = new MagnifiedSize(Buffer.Size, m);
            this.canvas.Size = this.PaintAreaSize.ToSize();
            ResetLocation();
            this.Refresh();
            //colorBox.Invalidate();
        }
        public void ChangeImageBlender(IImageBlender b)
        {
            PenCase.Blender = b;
        }


        private IImageTransfer imageTransfer = new DirectImageTransfer();
        public void ChangeImageTransfer(IImageTransfer i)
        {
            imageTransfer = i;
            Refresh();
        }

        private void PaintUpdate(Graphics g)
        {
            IPaintLayer layer = new PaintBackgroundLayer(CanvasBackgroundService.Instance);
            layer = new PaintBufferLayer(layer, PaintAreaSize, Buffer.Bmp, imageTransfer);
            layer = new PaintGridLayer(layer, PaintAreaSize, new MagnifiedSize(new Size(16, 16), m));
            layer.Paint(g);
        }

        private void ResetLocation()
        {
            Point now = this.AutoScrollPosition;
            int newWidth = now.X;
            if (this.canvas.Width < this.Width)
            {
                newWidth = (this.Width / 2) - (this.canvas.Width / 2);
            }

            int newHeight = now.Y;
            if (this.canvas.Height < this.Height)
            {
                newHeight = (this.Height / 2) - (this.canvas.Height / 2);
            }

            canvas.Location = new Point(newWidth, newHeight);
        }

        #region イベント
        private void PaintableBox_SizeChanged(object sender, EventArgs e)
        {
            // Buffer.Bmp = new Bitmap(Buffer.Bmp, colorBox.Width, colorBox.Height);
            ResetLocation();
        }

        public event EventHandler ColorChanged;
        private void fireColorChanged()
        {
            this.ColorChanged?.Invoke(this, EventArgs.Empty);
        }

        private PositionHistory PositionHistory = null;

        private void colorBox_MouseDown(object sender, MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (IsDrawing) return;

                    var pos = new MinifiedPosition(new Position(e.X, e.Y), m).ToPosition();
                    if (Buffer.IsInnerBitmap(pos))
                    {
                        //TODO:DrawBuffer保存
                        IsDrawing = true;

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
                    if (IsDrawing)
                    {
                        //描画をキャンセルする
                        //TODO:DrawBufferhukki
                    }
                    else
                    {
                        //色を拾う
                        SetPenColor(DropColor(new MinifiedPosition(new Position(e.X, e.Y), m)));
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

        private void colorBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsDrawing) return;

            var pos = new MinifiedPosition(new Position(e.X, e.Y), m);
            UpdatePositionHistory(pos);
            PenStyle.Drawing(Buffer, PenCase, PositionHistory, IsShift());
            canvas.Invalidate();

        }

        private void colorBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (!IsDrawing) return;

            var pos = new MinifiedPosition(new Position(e.X, e.Y), m);
            UpdatePositionHistory(pos);

            PenStyle.DrawEnd(Buffer, PenCase, PositionHistory, IsShift());
            IsDrawing = false;
        }

        private void colorBox_MouseEnter(object sender, EventArgs e)
        {
        }

        private void colorBox_MouseLeave(object sender, EventArgs e)
        {
        }

        private void colorBox_Paint(object sender, PaintEventArgs e)
        {
            PaintUpdate(e.Graphics);
        }

        #endregion
    }

}
