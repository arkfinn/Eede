using Avalonia.Win32.Interoperability;
using System;
using System.Windows.Forms;

namespace Eede.Ui.AvaloniaWrapper.Navigation
{
    internal partial class DrawStyleMenu : UserControl
    {
        private readonly WinFormsAvaloniaControlHost Host = new();
        public readonly Views.Navigation.DrawStyleMenu Element = new();

        public DrawStyleMenu()
        {
            InitializeComponent();
        }

        private void DrawStyleMenu_Load(object sender, EventArgs e)
        {
            Host.Dock = DockStyle.Fill;
            Host.Content = Element;
            panel1.Controls.Add(Host);
        }

    }
}
