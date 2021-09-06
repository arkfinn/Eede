using System;
using System.ComponentModel;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Eede.Ui
{
    public partial class BoxSizeSettingDialog : Form
    {
        public BoxSizeSettingDialog()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void comboBox_Validating(object sender, CancelEventArgs e)
        {
            var regex = new Regex(@"^[0-9]+$");
            var c = sender as ComboBox;
            if (!regex.IsMatch(c.Text) || int.Parse(c.Text) <= 0)
            {
                e.Cancel = true;
            }
        }

        public void SetBoxSize(Size size)
        {
            comboBox1.Text = size.Width.ToString();
            comboBox2.Text = size.Height.ToString();
        }

        public Size GetInputBoxSize()
        {
            return new Size(int.Parse(comboBox1.Text), int.Parse(comboBox2.Text));
        }
    }
}
