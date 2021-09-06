using System.Drawing;
using System.Windows.Forms;
using System;

namespace Eede.Ui
{
    public partial class PenSizeSetter : UserControl
    {

        public PenSizeSetter()
        {
            InitializeComponent();
           
            SetupScrollBar();
            RefleshPenSize();
        }

        public int PenSize
        {
            get { return pictureBox1.PenSize; }
            set { pictureBox1.PenSize = value; }
        }

        public event EventHandler PenSizeChanged;
        private void InvokePenSizeChanged()
        {
            this.PenSizeChanged?.Invoke(this, EventArgs.Empty);
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!('0' <= e.KeyChar && e.KeyChar <= '9')) e.Handled = true;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int res;
            if (!int.TryParse(textBox1.Text, out res))
            {
                res = pictureBox1.MinimumPenSize;
            }
            pictureBox1.PenSize = res;
        }

        private void RefleshPenSize()
        {
            textBox1.Text = pictureBox1.PenSize.ToString();
            vScrollBar1.Value = Math.Min(vScrollBar1.Maximum, pictureBox1.PenSize);
        }

        private void pictureBox1_PenSizeChanged(object sender, EventArgs e)
        {
            RefleshPenSize();
            InvokePenSizeChanged();
        }

        private void SetupScrollBar()
        {
            vScrollBar1.Maximum = pictureBox1.Height - 3 + vScrollBar1.LargeChange;
        }

        private void PenSizeSetter_Resize(object sender, EventArgs e)
        {
            SetupScrollBar();
        }

        private void vScrollBar1_ValueChanged(object sender, EventArgs e)
        {
            pictureBox1.PenSize = vScrollBar1.Value;
        }




    }
}
