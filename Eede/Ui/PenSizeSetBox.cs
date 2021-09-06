using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Eede.Ui
{
    class PenSizeSetBox : System.Windows.Forms.PictureBox
    {
        Image backBuf=null;

        public PenSizeSetBox()
            : base()
        {

            this.Disposed += DoDisposed;
            this.SizeChanged += DoSizeChanged;

            this.MouseDown += DoMouseDown;
            this.MouseMove += DoMouseMove;
            this.MouseUp += DoMouseUp;

            SetupBackBuffer();
            UpdatePictureBoxBuffer();
        }


        private void DoDisposed(object sender, EventArgs e)
        {
            if (backBuf != null) backBuf.Dispose();
        }

        private void SetupBackBuffer()
        {
            if (backBuf != null) backBuf.Dispose();
            backBuf = new Bitmap(Width, Height);
            this.Image = backBuf;
        }

        private void UpdatePictureBoxBuffer()
        {
            using (Graphics g = Graphics.FromImage(backBuf))
            {
                g.DrawRectangle(Pens.Black, 0, 0, Width - 1, Height - 1);
                g.FillRectangle(Brushes.Black, 1, 1, Width - 2, PenSize);
                g.FillRectangle(Brushes.White, 1, 1 + PenSize, Width - 2, Height - 2 - PenSize);
            }
            Refresh();
        }

        private void DoSizeChanged(object sender, EventArgs e)
        {

            if (backBuf.Width < Width || backBuf.Height < Height)
            {
                SetupBackBuffer();
            }
            UpdatePictureBoxBuffer();
        }

        private int penSize = 1;
        public int PenSize
        {
            get { return penSize; }
            set
            {
                if (penSize != value)
                {
                    if (0 < MaximumPenSize) value = Math.Max(value, MaximumPenSize);
                    penSize = Math.Max(MinimumPenSize, value);
                    UpdatePictureBoxBuffer();
                    InvokePenSizeChanged();
                }
            }
        }

        private int minimumPenSize = 1;

        public int MinimumPenSize
        {
            get { return minimumPenSize; }
            set { minimumPenSize = Math.Max(1, value); }
        }

        private int maximumPenSize = -1;

        /// <summary>
        /// PenSizeの最大値。-1で無制限
        /// </summary>
        public int MaximumPenSize
        {
            get { return maximumPenSize; }
            set
            {
                maximumPenSize = value;
                if (maximumPenSize <= 0) maximumPenSize = -1;
            }
        }

        private int oldPenSize = 1;
        private bool drag = false;
        private void DoMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    oldPenSize = PenSize;
                    PenSize = e.Y - 1;
                    drag = true;
                    break;
                case MouseButtons.Middle:
                    break;
                case MouseButtons.None:
                    break;
                case MouseButtons.Right:
                    if (drag)
                    {
                        PenSize = oldPenSize;
                        drag = false;
                    }
                    break;
                case MouseButtons.XButton1:
                    break;
                case MouseButtons.XButton2:
                    break;
                default:
                    break;
            }
        }

        private void DoMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (drag)
                    {
                        if (e.Y < Height - 1 || (Control.ModifierKeys & Keys.Shift) == Keys.Shift)
                        {
                            PenSize = e.Y - 1;
                        }
                        else
                        {
                            PenSize = Height - 2;
                        }
                    }
                    break;
                case MouseButtons.Middle:
                    break;
                case MouseButtons.None:
                    break;
                case MouseButtons.Right:

                    break;
                case MouseButtons.XButton1:
                    break;
                case MouseButtons.XButton2:
                    break;
                default:
                    break;
            }
        }
        private void DoMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (drag)
                    {
                        drag = false;
                    }
                    break;
                case MouseButtons.Middle:
                    break;
                case MouseButtons.None:
                    break;
                case MouseButtons.Right:

                    break;
                case MouseButtons.XButton1:
                    break;
                case MouseButtons.XButton2:
                    break;
                default:
                    break;
            }
        }

        public event EventHandler PenSizeChanged;

        public void InvokePenSizeChanged()
        {
            if (PenSizeChanged != null)
            {
                PenSizeChanged(this, EventArgs.Empty);
            }
        }

    }

}
