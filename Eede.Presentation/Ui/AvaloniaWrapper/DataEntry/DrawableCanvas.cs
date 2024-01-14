using Avalonia.Win32.Interoperability;
using System;
using System.Windows.Forms;

namespace Eede.Ui.AvaloniaWrapper.DataEntry
{
    public partial class DrawableCanvas : UserControl
    {
        public DrawableCanvas()
        {
            InitializeComponent();
        }

        private readonly WinFormsAvaloniaControlHost Host = new();
        public readonly Views.DataEntry.DrawableCanvas Element = new();

        private void DrawableArea_Load(object sender, EventArgs e)
        {
            Host.Dock = DockStyle.Fill;
            Host.Content = Element;
            panel1.Controls.Add(Host);
        }
    }
}
