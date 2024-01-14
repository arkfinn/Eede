using Avalonia.Rendering;
using Avalonia.Win32.Interoperability;
using System;
using System.Windows.Forms;
using System.Windows.Input;

namespace Eede.Ui.AvaloniaWrapper.Navigation
{
    public partial class PictureActionMenu : UserControl
    {
        private readonly WinFormsAvaloniaControlHost Host = new();
        private readonly Views.Navigation.PictureActionMenu Element = new();


        public PictureActionMenu()
        {
            InitializeComponent();
        }

        private void PictureActionMenu_Load(object sender, EventArgs e)
        {
            Host.Dock = DockStyle.Fill;
            Host.Content = Element;
            panel1.Controls.Add(Host);
        }

        public ICommand? Command
        {
            get { return Element.Command; }
            set { Element.Command = value; }
        }

    }
}
