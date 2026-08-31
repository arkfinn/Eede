using Avalonia.Controls;
using Eede.Presentation.ViewModels.Pages;
using ReactiveUI.Avalonia;

namespace Eede.Presentation.Views.Pages;

public partial class MainWindow : ReactiveWindow<MainViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
    }
}
