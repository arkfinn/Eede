using Avalonia.Win32.Interoperability;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eede.Ui.AvaloniaWrapper.DataDisplay
{
    internal partial class PenWidthSelector : UserControl
    {
        public PenWidthSelector()
        {
            InitializeComponent();
        }

        private readonly WinFormsAvaloniaControlHost Host = new();
        public readonly Views.DataEntry.PenWidthSelector Element = new();

        private void PenWidthSelector_Load(object sender, EventArgs e)
        {
            Host.Dock = DockStyle.Fill;
            Host.Content = Element;
            panel1.Controls.Add(Host);
        }

    }
}
