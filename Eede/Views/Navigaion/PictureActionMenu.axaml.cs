using Avalonia;
using Avalonia.Controls;
using Eede.Common.Drawings;
using System.Windows.Input;

namespace Eede.Views.Navigaion
{
    /// <summary>
    /// PictureActionMenu.xaml の相互作用ロジック
    /// </summary>
    public partial class PictureActionMenu : UserControl
    {
        public PictureActionMenu()
        {
            InitializeComponent();
        }

        public static readonly StyledProperty<ICommand?> CommandProperty = 
            AvaloniaProperty.Register<PictureActionMenu, ICommand?>(nameof(Command));

        public ICommand? Command
        {
            get { return GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
    }
}
