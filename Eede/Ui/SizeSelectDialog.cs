using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public Size PictureSize
        {
            get
            {
                return new Size(Decimal.ToInt32(numericUpDown1.Value), Decimal.ToInt32(numericUpDown2.Value));
            }
            set
            {
                numericUpDown1.Value = value.Width;
                numericUpDown2.Value = value.Height;
            }
        }
    }
}
