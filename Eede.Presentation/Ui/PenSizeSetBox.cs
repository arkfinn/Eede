using System;
using System.Drawing;
using System.Windows.Forms;

namespace Eede.Ui
{
    internal class PenSizeSetBox : System.Windows.Forms.PictureBox
    {
        private Image backBuf = null;

        public PenSizeSetBox()
            : base()
        {

            Disposed += DoDisposed;
            SizeChanged += DoSizeChanged;

            MouseDown += DoMouseDown;
            MouseMove += DoMouseMove;
            MouseUp += DoMouseUp;

            SetupBackBuffer();
            UpdatePictureBoxBuffer();
        }


        private void DoDisposed(object sender, EventArgs e)
        {
            backBuf?.Dispose();
        }

        private void SetupBackBuffer()
        {
            backBuf?.Dispose();
            backBuf = new Bitmap(Width, Height);
            Image = backBuf;
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
            get => penSize;
            set
            {
                if (penSize != value)
                {
                    if (0 < MaximumPenSize)
                    {
                        value = Math.Max(value, MaximumPenSize);
                    }

                    penSize = Math.Max(MinimumPenSize, value);
                    UpdatePictureBoxBuffer();
                    InvokePenSizeChanged();
                }
            }
        }

        private int minimumPenSize = 1;

        public int MinimumPenSize
        {
            get => minimumPenSize;
            set => minimumPenSize = Math.Max(1, value);
        }

        private int maximumPenSize = -1;

        /// <summary>
        /// PenSizeの最大値。-1で無制限
        /// </summary>
        public int MaximumPenSize
        {
            get => maximumPenSize;
            set
            {
                maximumPenSize = value;
                if (maximumPenSize <= 0)
                {
                    maximumPenSize = -1;
                }
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
                        PenSize = e.Y < Height - 1 || (Control.ModifierKeys & Keys.Shift) == Keys.Shift ? e.Y - 1 : Height - 2;
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
            PenSizeChanged?.Invoke(this, EventArgs.Empty);
        }

    }

}
