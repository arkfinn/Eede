using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace Eede.Ui
{
    public partial class ColorPickerBox : UserControl
    {
        public ColorPickerBox()
        {
            InitializeComponent();
        }

        private void vGradationSlider1_ValueChanged(object sender, EventArgs e)
        {
            textBox1.Text = (Maximum - vGradationSlider1.Value).ToString();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            int res;
            if (!int.TryParse(textBox1.Text, out res))
            {
                res = Maximum;
            }
            vGradationSlider1.Value = Maximum - res;
        }


        public int Value
        {
            get { return Maximum - vGradationSlider1.Value; }
            set { textBox1.Text = value.ToString(); }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            vGradationSlider1.Value = vGradationSlider1.Value - 1;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            vGradationSlider1.Value = vGradationSlider1.Value + 1;

        }


        public Color[] GradationColor
        {
            get { return vGradationSlider1.GradationColor; }
            set { vGradationSlider1.GradationColor = value; }
        }

        public event EventHandler ValueChanged
        {
            add { vGradationSlider1.ValueChanged += value; }
            remove { vGradationSlider1.ValueChanged -= value; }
        }

 
        public int Maximum
        {
            get { return vGradationSlider1.Maximum; }
            set { vGradationSlider1.Maximum = value; }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!('0' <= e.KeyChar && e.KeyChar <= '9')) e.Handled = true;
  
        }


    }
}
