using Avalonia;
using Avalonia.Controls;
using System.Windows.Input;

namespace Eede.Views.Navigation
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
            get => GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
    }
}
