using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Eede.Common.Enums;
using Eede.Presentation.Common.Services;
using Eede.ViewModels.Pages;
using ReactiveUI;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eede.Views.Pages;

public partial class MainView : ReactiveUserControl<MainViewModel>
{
    public MainView()
    {
        InitializeComponent();
        StorageService = new StorageService(new Window().StorageProvider);



        DataContextChanged += (sender, e) =>
        {
            if (DataContext is not MainViewModel viewModel)
            {
                return;
            }
            AddHandler(DragDrop.DragOverEvent, viewModel.DragOverPicture);
            AddHandler(DragDrop.DropEvent, viewModel.DropPicture);
        };
    }

    public StorageService StorageService { get; private set; }

    public void OnClickThemeSelect(object? sender, SelectionChangedEventArgs e)
    {
        var app = Avalonia.Application.Current;
        if (app is null) return;
        switch (ThemeSelect?.SelectedIndex)
        {
            case 0:
                app.RequestedThemeVariant = ThemeVariant.Light;
                break;
            case 1:
                app.RequestedThemeVariant = ThemeVariant.Dark;
                break;
        }
    }
}
