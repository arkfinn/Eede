using System;
using System.Drawing;
using System.Windows.Forms;

namespace Eede.Ui
{
    public partial class SizeSelectDialog : Form
    {
        public SizeSelectDialog()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        public Size PictureSize
        {
            get => new(decimal.ToInt32(numericUpDown1.Value), decimal.ToInt32(numericUpDown2.Value));
            set
            {
                numericUpDown1.Value = value.Width;
                numericUpDown2.Value = value.Height;
            }
        }
    }
}
